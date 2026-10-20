import { PlexServerConnectionPaths } from '@api-urls';
import { route } from '@fixtures';
import { JobStatus, MessageTypes, PlexConnectionTypes, type PlexServerConnectionDTO, type ServerConnectionCheckStatusProgressDTO, type UpdatePlexServerConnectionEndpointRequest } from '@dto';
import { generatePlexServer, generatePlexServerConnection, generateResultDTO } from '@mock';

describe('Check server connections dialog', () => {
	before(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 3,
			maxServerConnections: 3,
			override: {
				plexServer: (plexServers) => [
					...plexServers,
					generatePlexServer({ id: 10, partialData: { name: 'No Connections Server' } }),
				],
				plexServerConnections: (plexServerConnections) => plexServerConnections.filter((x) => x.plexServerId !== 10),
			},
		});
		cy.visit(route('/empty'));
	});

	it('Should display the check server connections dialog when given the back-end signal', () => {
		cy.getPageData().then(({ plexServers, plexServerConnections }) => {
			const serversUnderTest = plexServers.slice(0, 3);
			const successServer = serversUnderTest[0]!;
			const failedServer = serversUnderTest[1]!;
			const loadingServer = serversUnderTest[2]!;
			const getConnections = (serverId: number) => plexServerConnections.filter((x) => x.plexServerId === serverId);
			const successConnections = getConnections(successServer.id);
			const failedConnections = getConnections(failedServer.id);
			const loadingConnections = getConnections(loadingServer.id);
			if (successConnections.length < 2 || failedConnections.length === 0 || loadingConnections.length < 2) {
				throw new Error('Expected success/loading servers to have at least two connections.');
			}
			const loadingInProgressConnection = loadingConnections[1]!;

			for (const server of serversUnderTest) {
				cy.hubPublishInspectPlexServerJob(JobStatus.Started, [server.id]);
			}

			cy.getCy('check-server-connection-dialog').should('exist').and('be.visible');

			cy.getCy('check-server-connection-dialog-progress')
				.should('have.attr', 'data-completed')
				.and('not.equal', 'true');

			const publishProgress = (
				connectionId: number,
				serverId: number,
				{
					completed,
					connectionSuccessful,
					statusCode,
				}: {
					completed: boolean;
					connectionSuccessful: boolean;
					statusCode: number;
				},
			) =>
				cy.hubPublish('progress', MessageTypes.ServerConnectionCheckStatusProgress, {
					plexServerId: serverId,
					plexServerConnectionId: connectionId,
					connectionSuccessful,
					statusCode,
					completed,
				} as Partial<ServerConnectionCheckStatusProgressDTO>);

			// Success server: one connectable, others still in progress
			publishProgress(successConnections[0]!.id, successServer.id, {
				completed: true,
				connectionSuccessful: true,
				statusCode: 200,
			});
			for (const connection of successConnections.slice(1)) {
				publishProgress(connection.id, successServer.id, {
					completed: false,
					connectionSuccessful: false,
					statusCode: 0,
				});
			}

			// Failed server: all failed and completed
			for (const connection of failedConnections) {
				publishProgress(connection.id, failedServer.id, {
					completed: true,
					connectionSuccessful: false,
					statusCode: 503,
				});
			}

			// Loading server: only failures, at least one still in progress
			publishProgress(loadingConnections[0]!.id, loadingServer.id, {
				completed: true,
				connectionSuccessful: false,
				statusCode: 503,
			});
			for (const connection of loadingConnections.slice(1)) {
				publishProgress(connection.id, loadingServer.id, {
					completed: false,
					connectionSuccessful: false,
					statusCode: 0,
				});
			}

			cy.getCy(`check-server-connections-dialog-server-title-${successServer.id}`).should('exist');
			cy.getCy(`check-server-connections-dialog-server-title-${failedServer.id}`).should('exist');
			cy.getCy(`check-server-connections-dialog-server-title-${loadingServer.id}`).should('exist');
			cy.getCy(`check-server-connections-dialog-result-text-success-${successServer.id}`).should('exist');
			cy.getCy(`check-server-connections-dialog-result-text-completed-${failedServer.id}`).should('exist');
			cy.getCy(`check-server-connections-dialog-result-text-completed-${loadingServer.id}`).should('not.exist');
			cy.getCy('check-server-connection-dialog-progress')
				.should('have.attr', 'data-completed')
				.and('equal', 'false');
			cy.contains('Checking 2 of 3 Plex servers connections').should('be.visible');
			cy.getCy(`check-server-connections-dialog-connection-title-${loadingInProgressConnection.id}`)
				.closest('.q-tree__node')
				.find(`[data-cy="check-server-connections-dialog-${loadingInProgressConnection.id}"]`)
				.should('exist');
			cy.getCy(`check-server-connections-dialog-server-title-${loadingServer.id}`)
				.closest('.q-tree__node')
				.find(`[data-cy="check-server-connections-dialog-${loadingServer.id}"]`)
				.should('exist');

			for (const connection of successConnections.slice(1)) {
				cy.hubPublish('progress', MessageTypes.ServerConnectionCheckStatusProgress, {
					plexServerId: connection.plexServerId,
					plexServerConnectionId: connection.id,
					connectionSuccessful: false,
					statusCode: 503,
					completed: true,
				} as Partial<ServerConnectionCheckStatusProgressDTO>);
			}
			for (const connection of loadingConnections.slice(1)) {
				cy.hubPublish('progress', MessageTypes.ServerConnectionCheckStatusProgress, {
					plexServerId: connection.plexServerId,
					plexServerConnectionId: connection.id,
					connectionSuccessful: false,
					statusCode: 503,
					completed: true,
				} as Partial<ServerConnectionCheckStatusProgressDTO>);
			}

			cy.getCy('check-server-connection-dialog-progress')
				.should('have.attr', 'data-completed')
				.and('equal', 'true');

			const allConnections = [...successConnections, ...failedConnections, ...loadingConnections];
			for (const connection of allConnections) {
				cy.getCy(`check-server-connections-dialog-connection-title-${connection.id}`)
					.closest('.q-tree__node')
					.find(`[data-cy="check-server-connections-dialog-${connection.id}"]`)
					.should('not.exist');
			}

			cy.getCy(`check-server-connections-dialog-result-text-completed-${loadingServer.id}`).should('exist');
		});
	});
});

type CustomUrlMutation = 'create' | 'update';

const customUrlActions: Record<CustomUrlMutation, { label: string; alias: string; method: string }> = {
	create: { label: 'Create Connection', alias: '@createConnection', method: 'POST' },
	update: { label: 'Update Connection', alias: '@updateConnection', method: 'PATCH' },
};

interface CustomUrlScenario {
	invalidUrls: readonly string[];
	createInputUrl: string;
	createUrl: string;
	updateInputUrl: string;
	updateUrl: string;
	connectionId: number;
	serverId: number;
	connections: PlexServerConnectionDTO[];
	calls: Record<CustomUrlMutation | 'validate', number>;
}

function createCustomUrlScenario(): CustomUrlScenario {
	return {
		invalidUrls: ['ftp://plex.example.test:32400/path', 'not-a-valid-url'],
		createInputUrl: 'https://plex.example.test:32400/path',
		createUrl: 'https://plex.example.test:32400',
		updateInputUrl: 'https://plex.example.test:32401/edited-path',
		updateUrl: 'https://plex.example.test:32401',
		connectionId: 9001,
		serverId: 0,
		connections: [],
		calls: { create: 0, update: 0, validate: 0 },
	};
}

function installCustomUrlApis(scenario: CustomUrlScenario): void {
	cy.basePageSetup({ plexAccountCount: 1, plexServerCount: 1, maxServerConnections: 0 });
	cy.intercept('GET', PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint(), (request) => {
		request.reply({ statusCode: 200, body: generateResultDTO(scenario.connections) });
	}).as('getConnections');
	cy.intercept('POST', PlexServerConnectionPaths.createPlexServerConnectionEndpoint(), (request) => {
		scenario.calls.create++;
		const connection = generatePlexServerConnection({
			id: scenario.connectionId,
			plexServerId: scenario.serverId,
			partialData: {
				protocol: 'https', address: 'plex.example.test', port: 32400,
				url: scenario.createUrl, isCustom: true, type: PlexConnectionTypes.Public,
			},
		});
		scenario.connections = [connection];
		request.reply({ statusCode: 200, body: generateResultDTO(connection) });
	}).as('createConnection');
	cy.intercept('PATCH', PlexServerConnectionPaths.updatePlexServerConnectionEndpoint(), (request) => {
		const body = request.body as UpdatePlexServerConnectionEndpointRequest;
		scenario.calls.update++;
		const connection = generatePlexServerConnection({
			id: body.id,
			plexServerId: body.plexServerId,
			partialData: { ...body, isCustom: true, type: PlexConnectionTypes.Public },
		});
		scenario.connections = [connection];
		request.reply({ statusCode: 200, body: generateResultDTO(connection) });
	}).as('updateConnection');
	cy.intercept('POST', PlexServerConnectionPaths.validatePlexServerConnectionEndpoint(), (request) => {
		scenario.calls.validate++;
		request.reply({ statusCode: 200, body: generateResultDTO({ version: '1.0.0', machineIdentifier: 'machine-id' }) });
	}).as('validateConnection');
}

function openCustomUrlCreateDialog(scenario: CustomUrlScenario): void {
	cy.visitEmptyPage();
	cy.getPageData().then(({ plexServers }) => {
		scenario.serverId = plexServers[0]!.id;
	});
	cy.wait('@getConnections');
	cy.getCy('server-dialog-0').filter(':visible').first().click();
	cy.getCy('server-dialog-cy').should('be.visible');
	cy.getCy('server-dialog-tab-2').click();
	cy.getCy('server-dialog-tab-content-2').should('be.visible');
	cy.contains('button', 'Add connection').click();
	cy.getCy('q-card-dialog-cy').should('be.visible');
}

function assertInvalidCustomUrls(scenario: CustomUrlScenario, mutation: CustomUrlMutation): void {
	const { label } = customUrlActions[mutation];
	for (const invalidUrl of scenario.invalidUrls) {
		cy.getCy('q-card-dialog-cy').find('input').first().clear().type(invalidUrl).blur();
		cy.getCy('q-card-dialog-cy').find('.invalid-icon').should('be.visible');
		cy.getCy('q-card-dialog-cy').contains('button', label).should('be.disabled');
		cy.then(() => expect(scenario.calls[mutation]).to.equal(0));
	}
}

function saveCustomUrl(scenario: CustomUrlScenario, mutation: CustomUrlMutation): void {
	const isCreate = mutation === 'create';
	const action = customUrlActions[mutation];
	const inputUrl = isCreate ? scenario.createInputUrl : scenario.updateInputUrl;
	const savedUrl = isCreate ? scenario.createUrl : scenario.updateUrl;
	cy.getCy('q-card-dialog-cy').find('input').first().clear().type(inputUrl).blur();
	cy.getCy('q-card-dialog-cy').find('input').first().should('have.value', inputUrl);
	cy.getCy('q-card-dialog-cy').find('.valid-icon').should('be.visible');
	cy.getCy('q-card-dialog-cy').contains('button', action.label)
		.should('not.be.disabled').click();
	cy.wait(action.alias).then(({ request, response }) => {
		const expectedBody = isCreate
			? { protocol: 'https', address: 'plex.example.test', port: 32400, url: savedUrl, plexServerId: scenario.serverId }
			: { id: scenario.connectionId, protocol: 'https', address: 'plex.example.test', port: 32401, url: savedUrl, plexServerId: scenario.serverId };
		expect(request.method).to.equal(action.method);
		expect(request.body).to.deep.equal(expectedBody);
		expect(response?.statusCode).to.equal(200);
		expect(response?.body.value).to.deep.include({ id: scenario.connectionId, url: savedUrl, isCustom: true });
	});
	cy.then(() => {
		expect(scenario.calls[mutation]).to.equal(1);
		expect(scenario.calls.validate).to.equal(0);
	});
	cy.getCy('q-card-dialog-cy').should('not.exist');
	cy.getCy('server-dialog-cy').should('be.visible').and('contain.text', savedUrl);
}

describe('Custom Plex connection URL validation', () => {
	it('rejects invalid protocols on add and edit, then saves valid HTTPS origins offline', () => {
		const scenario = createCustomUrlScenario();
		installCustomUrlApis(scenario);
		openCustomUrlCreateDialog(scenario);
		assertInvalidCustomUrls(scenario, 'create');
		saveCustomUrl(scenario, 'create');
		cy.contains('.server-connection-row', scenario.createUrl).find('.server-connection-row__edit button').click();
		cy.getCy('q-card-dialog-cy').should('be.visible');
		assertInvalidCustomUrls(scenario, 'update');
		saveCustomUrl(scenario, 'update');
		cy.getCy('server-dialog-cy').should('not.contain.text', scenario.createUrl);
	});
});
