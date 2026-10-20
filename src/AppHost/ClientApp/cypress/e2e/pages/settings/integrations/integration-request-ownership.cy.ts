import { IntegrationPaths } from '@api/generated/Integration';
import { route } from '@fixtures';
import {
	IntegrationProvisioningState,
	IntegrationType,
	TestConnectionStatus,
	type IntegrationSummary,
	type RadarrIntegrationDTO,
	type SonarrIntegrationDTO,
	type UpdateRadarrIntegrationRequest,
	type UpdateSonarrIntegrationRequest,
} from '@dto';
import { generateResultDTO } from '@mock';

type IntegrationDetail = SonarrIntegrationDTO | RadarrIntegrationDTO;
type SaveRequest = UpdateSonarrIntegrationRequest | UpdateRadarrIntegrationRequest;

type Provider = {
	name: 'Sonarr' | 'Radarr';
	type: IntegrationType;
	getDetail: (id: string) => string;
	update: (id: string) => string;
	setup: (id: string) => string;
	test: () => string;
};

const providers: readonly Provider[] = [
	{
		name: 'Sonarr',
		type: IntegrationType.Sonarr,
		getDetail: IntegrationPaths.getSonarrIntegrationEndpoint,
		update: IntegrationPaths.updateSonarrIntegrationEndpoint,
		setup: IntegrationPaths.setupSonarrIntegrationEndpoint,
		test: IntegrationPaths.testConnectionToSonarrEndpoint,
	},
	{
		name: 'Radarr',
		type: IntegrationType.Radarr,
		getDetail: IntegrationPaths.getRadarrIntegrationEndpoint,
		update: IntegrationPaths.updateRadarrIntegrationEndpoint,
		setup: IntegrationPaths.setupRadarrIntegrationEndpoint,
		test: IntegrationPaths.testConnectionToRadarrEndpoint,
	},
];

const testedAt = '2026-10-20T00:00:00.000Z';

type Gate = { release?: () => void };
type IntegrationRecord = {
	key: 'A' | 'B';
	summary: IntegrationSummary;
	detail: IntegrationDetail;
};
type Workflow = {
	records: [IntegrationRecord, IntegrationRecord];
	detailTargets: string[];
	saveTargets: string[];
	setupTargets: string[];
	detailGates: Record<IntegrationRecord['key'], Gate>;
	refreshGate: Gate;
	refreshHeld: boolean;
	saveGates: Record<IntegrationRecord['key'], Gate>;
};
type ApiOptions = {
	holdDetailIds?: string[];
	holdSaveIds?: string[];
	holdRefreshAfterSaveId?: string;
};
type WorkflowOptions = ApiOptions | ((workflow: Workflow) => ApiOptions);

function makeRecord(provider: Provider, key: 'A' | 'B'): IntegrationRecord {
	const lowerName = provider.name.toLowerCase();
	const lowerKey = key.toLowerCase();
	const name = `${provider.name} ${key}`;
	const url = `https://${lowerName}-${lowerKey}.example.test`;
	const detail: IntegrationDetail = {
		id: `00000000-0000-0000-0000-0000000000${provider.type === IntegrationType.Sonarr ? (key === 'A' ? '11' : '12') : (key === 'A' ? '21' : '22')}`,
		name,
		url,
		apiKey: `${lowerName}-${lowerKey}-api-key`,
		category: `reaparr-${lowerName}-${lowerKey}`,
		downloadFolderId: 1,
		provisioningState: IntegrationProvisioningState.Unconfigured,
		lastConnectionTestStatus: TestConnectionStatus.Unknown,
	};
	return {
		key,
		detail,
		summary: {
			id: detail.id,
			type: provider.type,
			name: detail.name,
			baseUrl: detail.url,
			category: detail.category,
			provisioningState: detail.provisioningState,
			lastConnectionTestStatus: detail.lastConnectionTestStatus,
			downloadFolderId: detail.downloadFolderId,
			externalDownloadClientId: null,
			externalIndexerId: null,
		},
	};
}

function createWorkflow(provider: Provider): Workflow {
	const records = [makeRecord(provider, 'A'), makeRecord(provider, 'B')] as [IntegrationRecord, IntegrationRecord];
	return {
		records,
		detailTargets: [],
		saveTargets: [],
		setupTargets: [],
		detailGates: { A: {}, B: {} },
		refreshGate: {},
		refreshHeld: false,
		saveGates: { A: {}, B: {} },
	};
}

function installApi(provider: Provider, workflow: Workflow, options: ApiOptions): void {
	cy.intercept('GET', IntegrationPaths.getIntegrationsEndpoint(), (request) => {
		const body = generateResultDTO(workflow.records.map(({ summary }) => summary));
		const shouldHoldRefresh = !workflow.refreshHeld
			&& options.holdRefreshAfterSaveId !== undefined
			&& workflow.saveTargets.includes(options.holdRefreshAfterSaveId);
		if (shouldHoldRefresh) {
			workflow.refreshHeld = true;
			request.alias = 'heldIntegrationRefresh';
			return new Promise<void>((resolve) => {
				workflow.refreshGate.release = () => {
					workflow.refreshGate.release = undefined;
					request.reply({ statusCode: 200, body });
					resolve();
				};
			});
		}
		if (workflow.saveTargets.length > 0) request.alias = 'postMutationRefresh';
		request.reply({ statusCode: 200, body });
	}).as('getIntegrations');

	for (const record of workflow.records) {
		installDetailApi(provider, workflow, options, record);

		cy.intercept('PUT', provider.update(record.detail.id), (request) => {
			const body = request.body as SaveRequest;
			const updatedDetail = { ...record.detail, ...body } as IntegrationDetail;
			const commit = () => {
				record.detail = updatedDetail;
				record.summary = {
					...record.summary,
					name: body.name,
					baseUrl: body.url,
					category: body.category,
					downloadFolderId: body.downloadFolderId,
				};
			};
			workflow.saveTargets.push(record.detail.id);
			const response = { statusCode: 200, body: generateResultDTO(updatedDetail) };
			if (options.holdSaveIds?.includes(record.detail.id)) {
				const gate = workflow.saveGates[record.key];
				return new Promise<void>((resolve) => {
					gate.release = () => {
						gate.release = undefined;
						commit();
						request.reply(response);
						resolve();
					};
				});
			}
			commit();
			request.reply(response);
		}).as(`save${record.key}`);

		cy.intercept('POST', provider.setup(record.detail.id), (request) => {
			workflow.setupTargets.push(record.detail.id);
			const configuredDetail = { ...record.detail, provisioningState: IntegrationProvisioningState.Configured } as IntegrationDetail;
			record.detail = configuredDetail;
			record.summary = { ...record.summary, provisioningState: IntegrationProvisioningState.Configured };
			request.reply({ statusCode: 200, body: generateResultDTO(configuredDetail) });
		}).as(`setup${record.key}`);
	}

	cy.intercept({ method: 'GET', pathname: provider.test() }, {
		statusCode: 200,
		body: generateResultDTO({
			result: TestConnectionStatus.Success,
			errorMessage: null,
			httpStatusCode: 200,
			testedAt,
		}),
	}).as('testConnection');
}

function installDetailApi(provider: Provider, workflow: Workflow, options: ApiOptions, record: IntegrationRecord): void {
	cy.intercept('GET', provider.getDetail(record.detail.id), (request) => {
		workflow.detailTargets.push(record.detail.id);
		if (options.holdDetailIds?.includes(record.detail.id)) {
			const gate = workflow.detailGates[record.key];
			return new Promise<void>((resolve) => {
				gate.release = () => {
					gate.release = undefined;
					request.reply({ statusCode: 200, body: generateResultDTO(record.detail) });
					resolve();
				};
			});
		}
		request.reply({ statusCode: 200, body: generateResultDTO(record.detail) });
	}).as(`getDetail${record.key}`);
}

function startWorkflow(provider: Provider, options: WorkflowOptions = {}): Workflow {
	const workflow = createWorkflow(provider);
	const apiOptions = typeof options === 'function' ? options(workflow) : options;
	cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 });
	installApi(provider, workflow, apiOptions);
	cy.visit(route('/settings/integrations'));
	cy.getPageData();
	cy.wait('@getIntegrations');
	return workflow;
}

function card(name: string) {
	return cy.getCy('integration-card').filter(`[aria-label="${name}"]`);
}

function waitForGate(gate: Gate): void {
	cy.wrap(null).should(() => expect(gate.release).to.be.a('function'));
}

function releaseGate(gate: Gate): void {
	cy.then(() => {
		expect(gate.release).to.be.a('function');
		gate.release?.();
	});
}

function waitForRequest(alias: string, id: string): void {
	cy.wait(alias).then(({ request }) => expect(request.url).to.contain(id));
}

for (const provider of providers) {
	describe(`${provider.name} integration request ownership`, () => {
		it('keeps the current edit after a closed session receives a late detail response', () => {
			const workflow = startWorkflow(provider, (scenario) => ({
				holdDetailIds: scenario.records.map((record) => record.detail.id),
			}));
			const [a, b] = workflow.records;
			card(a.summary.name).click();
			cy.getCy('integration-dialog').should('be.visible');
			waitForGate(workflow.detailGates[a.key]);
			cy.then(() => expect(workflow.detailTargets).to.deep.equal([a.detail.id]));
			cy.getCy('integration-dialog').find('[data-cy="dialog-close-button"]').click();
			cy.getCy('integration-dialog').should('not.exist');
			card(b.summary.name).click();
			waitForGate(workflow.detailGates[b.key]);
			cy.getCy('integration-name').should('have.value', '');
			cy.getCy('integration-base-url').should('have.value', '');
			cy.getCy('integration-save').should('be.disabled');
			releaseGate(workflow.detailGates[a.key]);
			waitForRequest('@getDetailA', a.detail.id);
			cy.getCy('integration-name').should('have.value', '');
			cy.getCy('integration-base-url').should('have.value', '');
			cy.getCy('integration-save').should('be.disabled');
			releaseGate(workflow.detailGates[b.key]);
			waitForRequest('@getDetailB', b.detail.id);
			cy.getCy('integration-dialog').should('be.visible');
			cy.getCy('integration-name').should('have.value', b.detail.name);
			cy.getCy('integration-base-url').should('have.value', b.detail.url);
			cy.getCy('integration-save-error').should('not.exist');
		});

		it('keeps B visible and busy while a late A save refreshes the list', () => {
			const workflow = startWorkflow(provider, (scenario) => ({
				holdRefreshAfterSaveId: scenario.records[0].detail.id,
				holdSaveIds: scenario.records.map((record) => record.detail.id),
			}));
			const [a, b] = workflow.records;
			const savedAName = `${a.summary.name} persisted`;
			const savedBName = `${b.summary.name} persisted`;
			card(a.summary.name).click();
			waitForRequest('@getDetailA', a.detail.id);
			cy.getCy('integration-name').clear().type(savedAName);
			cy.getCy('integration-save').click();
			cy.getCy('integration-save').find('.q-spinner').should('exist');
			waitForGate(workflow.saveGates[a.key]);
			cy.getCy('integration-dialog').find('[data-cy="dialog-close-button"]').click();
			card(b.summary.name).click();
			waitForRequest('@getDetailB', b.detail.id);
			cy.getCy('integration-name').clear().type(savedBName);
			cy.getCy('integration-save').click();
			cy.getCy('integration-save').find('.q-spinner').should('exist');
			waitForGate(workflow.saveGates[b.key]);
			cy.then(() => expect(workflow.saveTargets).to.deep.equal([a.detail.id, b.detail.id]));
			releaseGate(workflow.saveGates[a.key]);
			waitForRequest('@saveA', a.detail.id);
			waitForGate(workflow.refreshGate);
			cy.getCy('integration-name').should('have.value', savedBName);
			cy.getCy('integration-save').find('.q-spinner').should('exist');
			releaseGate(workflow.refreshGate);
			cy.wait('@heldIntegrationRefresh');
			cy.getCy('integration-dialog').should('be.visible');
			cy.getCy('integration-name').should('have.value', savedBName);
			cy.getCy('integration-base-url').should('have.value', b.detail.url);
			cy.getCy('integration-save').find('.q-spinner').should('exist');
			card(savedAName).should('contain.text', savedAName);
			releaseGate(workflow.saveGates[b.key]);
			waitForRequest('@saveB', b.detail.id);
			cy.wait('@postMutationRefresh');
			cy.getCy('integration-dialog').should('not.exist');
			card(savedAName).should('be.visible').and('contain.text', savedAName);
			card(savedBName).should('be.visible').and('contain.text', savedBName);
		});

		it('does not open setup for B from a late Save-and-setup A callback', () => {
			const workflow = startWorkflow(provider, (scenario) => ({
				holdRefreshAfterSaveId: scenario.records[0].detail.id,
				holdSaveIds: [scenario.records[0].detail.id],
			}));
			const [a, b] = workflow.records;
			const editedAName = `${a.summary.name} setup`;
			card(a.summary.name).click();
			waitForRequest('@getDetailA', a.detail.id);
			cy.getCy('integration-name').clear().type(editedAName);
			cy.getCy('integration-test').click();
			waitForRequest('@testConnection', a.detail.id);
			cy.getCy('integration-setup').should('not.be.disabled').click();
			cy.getCy('integration-setup').find('.q-spinner').should('exist');
			waitForGate(workflow.saveGates[a.key]);
			cy.getCy('integration-dialog').find('[data-cy="dialog-close-button"]').click();
			card(b.summary.name).click();
			waitForRequest('@getDetailB', b.detail.id);
			releaseGate(workflow.saveGates[a.key]);
			waitForRequest('@saveA', a.detail.id);
			waitForGate(workflow.refreshGate);
			cy.getCy('integration-dialog').should('be.visible');
			cy.getCy('integration-name').should('have.value', b.detail.name);
			releaseGate(workflow.refreshGate);
			cy.wait('@heldIntegrationRefresh');
			card(editedAName).should('contain.text', editedAName);
			cy.getCy('integration-dialog').should('be.visible');
			cy.getCy('integration-name').should('have.value', b.detail.name);
			cy.getCy('integration-setup-dialog').should('not.exist');
			cy.then(() => expect(workflow.setupTargets).to.deep.equal([]));
		});
	});
}
