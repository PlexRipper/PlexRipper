import { JobStatus, JobTypes, MessageTypes, type SyncServerMediaProgress } from '@dto';
import { generateSyncServerMediaProgress } from '@factories';

describe('SyncServerMediaDialog', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
		});

		cy.visitEmptyPage();
	});

	it('Should display the SyncServerMediaDialog when opening from the background activity button', () => {
		cy.getPageData().then((data) => {
			cy.hubPublishJobStatusUpdate(JobTypes.SyncServerMediaJob, JobStatus.Started, {
				plexServerId: data.plexServers[0].id,
				forceSync: false,
			});

			cy.getCy('background-activity-button').click();

			cy.getCy(JobTypes.SyncServerMediaJob + 'activity-button').click();
			cy.getCy('sync-server-media-dialog').should('exist').and('be.visible');

			for (let i = 0; i <= 10; i++) {
				const progress: SyncServerMediaProgress[] = [];

				for (const plexServer of data.plexServers) {
					progress.push(
						generateSyncServerMediaProgress({
							progressIndex: i,
							plexServerId: plexServer.id,
							plexLibraryIds: data.plexLibraries.filter((x) => x.plexServerId === plexServer.id).map((x) => x.id),
						}),
					);
				}

				cy.wait(500).hubPublish('progress', MessageTypes.SyncServerMediaProgress, progress);
				cy.log('progress', progress);
			}

			cy.hubPublishJobStatusUpdate(JobTypes.SyncServerMediaJob, JobStatus.Completed, {
				plexServerId: data.plexServers[0].id,
				forceSync: false,
			});

			cy.getCy('sync-server-media-dialog-hide-btn').click();
			cy.getCy('sync-server-media-dialog').should('not.exist');
		});
	});
});
