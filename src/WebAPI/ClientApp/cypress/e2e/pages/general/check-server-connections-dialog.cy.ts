import { route } from '@fixtures/baseE2E';
import { JobStatus, JobTypes, MessageTypes, type ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { generateJobStatusUpdate } from '@factories';

describe('Check server connections dialog', () => {
	before(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 3,
			maxServerConnections: 3,
		});
		cy.visit(route('/empty'));
	});

	it('Should display the check server connections dialog when given the back-end signal', function () {
		cy.getPageData()
			.then((data) => {
				cy.hubPublish(
					'progress',
					MessageTypes.JobStatusUpdate,
					generateJobStatusUpdate({
						jobType: JobTypes.InspectPlexServerByPlexAccountIdJob,
						jobStatus: JobStatus.Running,
						primaryKey: 'plexAccountId',
						primaryKeyValue: data.plexAccounts[0].id.toString(),
					}),
				);
			})
			.getCy('check-server-connection-dialog')
			.should('exist')
			.and('be.visible');

		cy.log('Should display the servers when the account has access to those Plex servers');

		cy.get('.q-card-dialog-content')
			.getPageData()
			.then((data) => {
				// Ensure the dialog is displaying the correct number of servers
				for (const plexServer of data.plexServers) {
					cy.findByText(plexServer.name, {
						selector: '[data-cy="check-server-connections-dialog-server-title"]',
					})
						.should('exist')
						.and('be.visible');
					// Ensure the dialog is displaying the correct number of connections per server
					for (const plexServerConnection of plexServer.plexServerConnections) {
						cy.findByText(plexServerConnection.url, {
							selector: '[data-cy="check-server-connections-dialog-connection-title"]',
						})
							.should('exist')
							.and('be.visible');
					}
				}
			});

		cy.getPageData().then((data) => {
			for (const plexServerConnection of data.plexServerConnections) {
				cy.wait(500).hubPublish('progress', MessageTypes.ServerConnectionCheckStatusProgress, {
					plexServerId: plexServerConnection.plexServerId,
					plexServerConnectionId: plexServerConnection.id,
					connectionSuccessful: true,
					statusCode: 200,
					completed: true,
				} as Partial<ServerConnectionCheckStatusProgressDTO>);
			}
		});

		cy.getCy('check-server-connection-dialog-hide-btn').click();
		cy.getCy('check-server-connection-dialog').should('not.exist');
	});
});
