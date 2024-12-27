import { route } from '@fixtures/baseE2E';
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
			cy.hubPublishInspectPlexServerJob(
				JobStatus.Started,
				plexServers.map((x) => x.id),
			);

			for (const plexServerConnection of plexServerConnections) {
				cy.wait(200).hubPublish('progress', MessageTypes.ServerConnectionCheckStatusProgress, {
					plexServerId: plexServerConnection.plexServerId,
					plexServerConnectionId: plexServerConnection.id,
					connectionSuccessful: true,
					statusCode: 200,
					completed: true,
				} as Partial<ServerConnectionCheckStatusProgressDTO>);
			}
		});

		cy.getCy('check-server-connection-dialog').should('exist').and('be.visible');

		cy.getCy('check-server-connection-dialog')
			.getPageData()
			.then(({ plexServers, plexServerConnections }) => {
				// Ensure the dialog is displaying the correct number of servers
				for (const plexServer of plexServers) {
					cy.findByText(plexServer.name, {
						selector: '[data-cy="check-server-connections-dialog-server-title"]',
					})
						.should('exist')
						.and('be.visible');
					// Ensure the dialog is displaying the correct number of connections per server
					const connections = plexServerConnections.filter((x) => x.plexServerId === plexServer.id);
					if (connections.length > 0) {
						for (const plexServerConnection of connections) {
							cy.findByText(plexServerConnection.url, {
								selector: '[data-cy="check-server-connections-dialog-connection-title"]',
							}).should('exist');
						}
					}
				}
			});

		cy.getCy('check-server-connection-dialog-hide-btn').click();
		cy.getCy('check-server-connection-dialog').should('not.exist');
	});
});
