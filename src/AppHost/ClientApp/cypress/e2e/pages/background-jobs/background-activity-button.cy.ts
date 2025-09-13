import { route } from '@fixtures';
import { JobStatus, JobTypes } from '@dto';

describe('Check BackgroundActivityToggleButton', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 3,
			maxServerConnections: 3,
		});
		cy.visit(route('/empty'));
	});

	it('Should display the background activity button loading when CheckPlexServerConnectionsJob is started', function () {
		cy.getPageData().then((data) => {
			cy.getCy('background-activity-button').should('exist').and('be.visible');
			cy.getCy('background-activity-button-badge').should('not.exist');
			cy.getCy('background-activity-button-loading').should('not.exist');

			cy.hubPublishCheckPlexServerConnectionsJob(JobStatus.Started, data.plexServers, data.plexServerConnections);
			cy.getCy('background-activity-button').click();

			cy.getCy('background-activity-button-badge').should('contain.text', 1);
			cy.getCy('background-activity-button-loading').should('be.exist');

			cy.getCy(JobTypes.CheckAllConnectionsStatusByPlexServerJob + 'activity-button').click();

			cy.getCy('check-server-connection-dialog').should('exist').and('be.visible');
		});
	});

	it('Should not display the background activity button loading when CheckPlexServerConnectionsJob is completed', function () {
		cy.getPageData().then((data) => {
			cy.hubPublishCheckPlexServerConnectionsJob(JobStatus.Started, data.plexServers, data.plexServerConnections);
			cy.wait(1000);
			cy.hubPublishCheckPlexServerConnectionsJob(JobStatus.Completed, data.plexServers, data.plexServerConnections);
			cy.getCy('background-activity-button').click();

			cy.getCy('background-activity-button-badge').should('not.exist');
			cy.getCy('background-activity-button-loading').should('not.exist');
		});
	});
});
