import { route } from '@fixtures';
import { JobStatus, MessageTypes, type ServerConnectionCheckStatusProgressDTO } from '@dto';
import { generatePlexServer } from '@mock';

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
