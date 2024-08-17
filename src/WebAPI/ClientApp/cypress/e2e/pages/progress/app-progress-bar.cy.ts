import { JobStatus, JobTypes, MessageTypes, type SyncServerMediaProgress } from '@dto';
import { generateSyncServerMediaProgress } from '@factories';

describe('Display the progress bar in the app bar', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
		});

		cy.visitEmptyPage();
	});

	it('Should navigate the server dialog tabs when the navigation tabs are used and then close again', () => {
		cy.getPageData().then((data) => {
			cy.hubPublishJobStatusUpdate(JobTypes.SyncServerMediaJob, JobStatus.Started, {
				plexServerId: data.plexServers[0].id,
				forceSync: false,
			});

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
		});
	});
});
