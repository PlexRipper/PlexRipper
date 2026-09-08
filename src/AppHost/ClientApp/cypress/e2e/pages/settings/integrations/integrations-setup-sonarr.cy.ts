import { IntegrationPaths } from '@api/generated/Integration';
import { route } from '@fixtures';
import {
	IntegrationProvisioningState,
	IntegrationSetupProgressStage,
	IntegrationType,
	MessageTypes,
	TestConnectionStatus,
	type IntegrationSetupProgressDTO,
	type IntegrationSummary,
	type SonarrIntegrationDTO,
} from '@dto';
import { generateResultDTO } from '@mock';

const integrationId = '00000000-0000-0000-0000-000000000001';
const createdIntegrationId = '00000000-0000-0000-0000-000000000002';
const integrationName = 'Sonarr';
const integrationUrl = 'http://localhost:8989';
const integrationApiKey = '0123456789abcdef0123456789abcdef';
const integrationCategory = 'sonarr';
const updatedName = 'Sonarr Updated';
const testedAt = '2026-01-01T00:00:00.000Z';
const createRequest = {
	name: 'Sonarr Created',
	url: 'http://localhost:9898',
	apiKey: integrationApiKey,
	category: 'sonarr-created',
	downloadFolderId: 1,
};
const unconfiguredSummary: IntegrationSummary = {
	id: integrationId,
	type: IntegrationType.Sonarr,
	name: integrationName,
	baseUrl: integrationUrl,
	category: integrationCategory,
	provisioningState: IntegrationProvisioningState.Unconfigured,
	lastConnectionTestStatus: TestConnectionStatus.Unknown,
	downloadFolderId: 0,
	externalDownloadClientId: null,
	externalIndexerId: null,
};
const unconfiguredDetail: SonarrIntegrationDTO = {
	id: integrationId,
	name: integrationName,
	url: integrationUrl,
	apiKey: integrationApiKey,
	category: integrationCategory,
	downloadFolderId: 0,
	provisioningState: IntegrationProvisioningState.Unconfigured,
	lastConnectionTestStatus: TestConnectionStatus.Unknown,
};
const setupStages = [
	{
		stage: IntegrationSetupProgressStage.Connecting,
		selector: 'connect',
		error: 'Sonarr connection failed',
		successText: 'Connected.',
	},
	{
		stage: IntegrationSetupProgressStage.DownloadClient,
		selector: 'download-client',
		error: 'Sonarr download client setup failed',
		successText: 'Download client in Sonarr ready.',
	},
	{
		stage: IntegrationSetupProgressStage.Indexer,
		selector: 'indexer',
		error: 'Sonarr indexer setup failed',
		successText: 'Indexer client in Sonarr ready.',
	},
	{
		stage: IntegrationSetupProgressStage.Validation,
		selector: 'validation',
		error: 'Sonarr validation failed',
		successText: 'Sonarr integration setup validated.',
	},
	{
		stage: IntegrationSetupProgressStage.Done,
		selector: 'done',
		error: '',
		successText: 'Integration setup complete.',
	},
] as const;

describe('Sonarr integrations', () => {
	it('creates a Sonarr integration through validation, connection testing, and save', () => {
		let integrations: IntegrationSummary[] = [];
		let saveCallCount = 0;
		const createdDetail: SonarrIntegrationDTO = {
			id: createdIntegrationId,
			name: createRequest.name,
			url: createRequest.url,
			apiKey: createRequest.apiKey,
			category: createRequest.category,
			downloadFolderId: createRequest.downloadFolderId,
			provisioningState: IntegrationProvisioningState.Unconfigured,
			lastConnectionTestStatus: TestConnectionStatus.Success,
		};
		const createdSummary: IntegrationSummary = {
			id: createdIntegrationId,
			type: IntegrationType.Sonarr,
			name: createRequest.name,
			baseUrl: createRequest.url,
			category: createRequest.category,
			provisioningState: IntegrationProvisioningState.Unconfigured,
			lastConnectionTestStatus: TestConnectionStatus.Success,
			downloadFolderId: createRequest.downloadFolderId,
			externalDownloadClientId: null,
			externalIndexerId: null,
		};

		cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 });
		cy.intercept('GET', IntegrationPaths.getIntegrationsEndpoint(), (request) => {
			request.reply({ statusCode: 200, body: generateResultDTO(integrations) });
		}).as('getIntegrations');
		cy.intercept({ method: 'GET', pathname: IntegrationPaths.testConnectionToSonarrEndpoint() }, {
			statusCode: 200,
			body: generateResultDTO({
				result: TestConnectionStatus.Success,
				errorMessage: null,
				httpStatusCode: 200,
				testedAt,
			}),
		}).as('testSonarrConnection');
		cy.intercept('POST', IntegrationPaths.createSonarrIntegrationEndpoint(), (request) => {
			saveCallCount++;
			integrations = [createdSummary];
			request.reply({ statusCode: 200, body: generateResultDTO(createdDetail) });
		}).as('createSonarr');
		cy.visit(route('/settings/integrations'));
		cy.wait('@getIntegrations');

		cy.getCy('add-integration').click();
		cy.getCy('integration-dialog').should('be.visible');
		cy.getCy('integration-type-sonarr').click();

		cy.getCy('integration-name').focus().blur();
		cy.getCy('integration-name').closest('.q-field').should('contain.text', 'Required');
		cy.getCy('integration-base-url').focus().blur();
		cy.getCy('integration-base-url').closest('.q-field').should('contain.text', 'Required');
		cy.getCy('integration-arr-key').focus().blur();
		cy.getCy('integration-arr-key').closest('.q-field').should('contain.text', 'Required');
		cy.getCy('integration-category').clear().blur();
		cy.getCy('integration-category').closest('.q-field').should('contain.text', 'Required');
		cy.getCy('integration-save').should('be.disabled');
		cy.getCy('integration-test').should('be.disabled');
		cy.then(() => expect(saveCallCount).to.equal(0));

		cy.getCy('integration-name').type(createRequest.name);
		cy.getCy('integration-base-url').type(createRequest.url);
		cy.getCy('integration-arr-key').type(createRequest.apiKey);
		cy.getCy('integration-category').type(createRequest.category);
		cy.getCy('integration-download-folder').click();
		cy.get('.q-menu').contains('.q-item', '/Downloads').click();
		cy.getCy('integration-save').should('not.be.disabled');
		cy.getCy('integration-test').should('not.be.disabled');

		cy.getCy('integration-test').click();
		cy.wait('@testSonarrConnection').then(({ request, response }) => {
			expect(request.method).to.equal('GET');
			expect(request.query).to.deep.include({ apiKey: createRequest.apiKey, url: createRequest.url });
			expect(response?.statusCode).to.equal(200);
		});
		cy.getCy('integration-dialog')
			.should('contain.text', 'Connection successful')
			.and('contain.text', 'HTTP 200');

		cy.getCy('integration-save').click();
		cy.wait('@createSonarr').then(({ request, response }) => {
			expect(request.method).to.equal('POST');
			expect(request.body).to.deep.equal(createRequest);
			expect(response?.statusCode).to.equal(200);
			expect(response?.body.value).to.deep.include(createdDetail);
		});
		cy.wait('@getIntegrations');
		cy.then(() => expect(saveCallCount).to.equal(1));

		cy.getCy('integration-dialog').find('[data-cy="dialog-close-button"]').click();
		cy.getCy('integration-dialog').should('not.exist');
		cy.getCy('integration-card')
			.should('have.length', 1)
			.and('be.visible')
			.and('have.attr', 'aria-label', createRequest.name)
			.and('contain.text', createRequest.name)
			.and('contain.text', createRequest.url)
			.and('contain.text', IntegrationProvisioningState.Unconfigured);
		cy.getCy('integration-card').find('[alt="sonarr"]').should('exist');
	});

	it('edits and sets up an existing unconfigured Sonarr integration', () => {
		let currentSummary = unconfiguredSummary;
		let currentDetail = unconfiguredDetail;
		let updateCallCount = 0;
		let setupCallCount = 0;
		const updateRequest = {
			name: updatedName,
			url: integrationUrl,
			apiKey: integrationApiKey,
			category: integrationCategory,
			downloadFolderId: 1,
		};
		const configuredSummary: IntegrationSummary = {
			...unconfiguredSummary,
			name: updatedName,
			downloadFolderId: 1,
			provisioningState: IntegrationProvisioningState.Configured,
			externalDownloadClientId: 1,
			externalIndexerId: 2,
		};
		const configuredDetail: SonarrIntegrationDTO = {
			...unconfiguredDetail,
			name: updatedName,
			downloadFolderId: 1,
			provisioningState: IntegrationProvisioningState.Configured,
		};

		cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 });
		cy.intercept('GET', IntegrationPaths.getIntegrationsEndpoint(), (request) => {
			request.reply({ statusCode: 200, body: generateResultDTO([currentSummary]) });
		}).as('getIntegrations');
		cy.intercept('GET', IntegrationPaths.getSonarrIntegrationEndpoint(integrationId), {
			statusCode: 200,
			body: generateResultDTO(currentDetail),
		}).as('getSonarr');
		cy.intercept('PUT', IntegrationPaths.updateSonarrIntegrationEndpoint(integrationId), (request) => {
			updateCallCount++;
			currentSummary = { ...currentSummary, name: request.body.name, baseUrl: request.body.url, category: request.body.category, downloadFolderId: request.body.downloadFolderId };
			currentDetail = { ...currentDetail, name: request.body.name, url: request.body.url, apiKey: request.body.apiKey, category: request.body.category, downloadFolderId: request.body.downloadFolderId };
			request.reply({ statusCode: 200, body: generateResultDTO(currentDetail) });
		}).as('updateSonarr');
		cy.intercept('POST', IntegrationPaths.setupSonarrIntegrationEndpoint(integrationId), (request) => {
			setupCallCount++;
			currentSummary = configuredSummary;
			currentDetail = configuredDetail;
			request.reply({ statusCode: 200, body: generateResultDTO(configuredDetail) });
		}).as('setupSonarr');
		cy.visit(route('/settings/integrations'));
		cy.wait('@getIntegrations');

		cy.getCy('integration-card').click();
		cy.wait('@getSonarr');
		cy.getCy('integration-name').should('have.value', integrationName);
		cy.getCy('integration-download-folder').click();
		cy.getCy('integration-name').click();
		cy.getCy('integration-download-folder').closest('.q-field').should('contain.text', 'Required');
		cy.getCy('integration-save').should('be.disabled');
		cy.getCy('integration-download-folder').click();
		cy.get('.q-menu').contains('.q-item', '/Downloads').click();
		cy.getCy('integration-name').invoke('val', updatedName).trigger('input').trigger('change');
		cy.getCy('integration-name').should('have.value', updatedName);
		cy.getCy('integration-save').click();
		cy.wait('@updateSonarr').then(({ request, response }) => {
			expect(request.method).to.equal('PUT');
			expect(request.body).to.deep.equal(updateRequest);
			expect(response?.statusCode).to.equal(200);
		});
		cy.wait('@getIntegrations');
		cy.then(() => expect(updateCallCount).to.equal(1));
		cy.getCy('integration-card').should('contain.text', updatedName).and('contain.text', IntegrationProvisioningState.Unconfigured);

		cy.getCy('integration-setup').click();
		cy.getCy('integration-setup-dialog').should('be.visible');
		for (const { selector } of setupStages) {
			cy.getCy(`integration-setup-step-${selector}`).should('have.attr', 'data-status', 'pending');
		}

		for (const [index, { stage, selector, error, successText }] of setupStages.entries()) {
			if (stage === IntegrationSetupProgressStage.Done) {
				cy.hubPublish('progress', MessageTypes.IntegrationSetupProgress, {
					integrationId,
					stage,
					isRunning: false,
					isSuccess: true,
				} satisfies IntegrationSetupProgressDTO);
				cy.getCy(`integration-setup-step-${selector}`)
					.should('have.attr', 'data-status', 'success')
					.and('contain.text', successText);
				continue;
			}

			cy.hubPublish('progress', MessageTypes.IntegrationSetupProgress, {
				integrationId,
				stage,
				isRunning: false,
				isSuccess: false,
				error,
			} satisfies IntegrationSetupProgressDTO);
			cy.getCy(`integration-setup-step-${selector}`)
				.should('have.attr', 'data-status', 'error')
				.and('contain.text', error);
			for (const { selector: previousSelector } of setupStages.slice(0, index)) {
				cy.getCy(`integration-setup-step-${previousSelector}`).should('have.attr', 'data-status', 'success');
			}
			for (const { selector: nextSelector } of setupStages.slice(index + 1)) {
				cy.getCy(`integration-setup-step-${nextSelector}`).should('have.attr', 'data-status', 'pending');
			}

			cy.hubPublish('progress', MessageTypes.IntegrationSetupProgress, {
				integrationId,
				stage,
				isRunning: false,
				isSuccess: true,
			} satisfies IntegrationSetupProgressDTO);
			cy.getCy(`integration-setup-step-${selector}`)
				.should('have.attr', 'data-status', 'success')
				.and('contain.text', successText);
		}

		cy.wait('@setupSonarr').then(({ request, response }) => {
			expect(request.method).to.equal('POST');
			expect(response?.statusCode).to.equal(200);
			expect(response?.body.value).to.deep.include(configuredDetail);
		});
		cy.wait('@getIntegrations');
		cy.then(() => expect(setupCallCount).to.equal(1));
		cy.getCy('integration-setup-dialog').find('[data-cy="dialog-close-button"]').click();
		cy.getCy('integration-setup-dialog').should('not.exist');
		cy.getCy('integration-dialog').find('[data-cy="dialog-close-button"]').click();
		cy.getCy('integration-dialog').should('not.exist');
		cy.getCy('integration-card')
			.should('be.visible')
			.and('have.attr', 'aria-label', updatedName)
			.and('contain.text', updatedName)
			.and('contain.text', IntegrationProvisioningState.Configured);
		cy.getCy('integration-card').find('[alt="sonarr"]').should('exist');
	});
});
