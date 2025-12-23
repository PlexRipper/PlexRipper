import {
	JobStatus,
	JobTypes,
	type LibraryProgress,
	type LibrarySyncJobQueueDTO,
	LibrarySyncJobStatus,
	MessageTypes,
} from '@dto';
import { generateLibraryProgress, generateLibrarySyncJobQueue } from '@factories';

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
			const server = data.plexServers[0]!;
			const serverLibraries = data.plexLibraries.filter((x) => x.plexServerId === server.id);

			// Start the sync job for each library on this server
			for (const library of serverLibraries) {
				cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
					JobTypes.LibrarySyncJob,
					JobStatus.Started,
					generateLibrarySyncJobQueue({
						plexLibraryId: library.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Processing,
					}),
				);
			}

			cy.getCy('background-activity-button').click();

			cy.getCy(JobTypes.LibrarySyncJob + 'activity-button').click();
			cy.getCy('sync-server-media-dialog').should('exist').and('be.visible');

			// Simulate progress updates
			for (let i = 0; i <= 10; i++) {
				const progress: LibraryProgress[] = serverLibraries.map((library) =>
					generateLibraryProgress({
						libraryId: library.id,
						received: i * 100,
						total: 1000,
					}),
				);

				cy.wait(500).hubPublish('progress', MessageTypes.LibraryProgress, progress);
			}

			// Complete the sync job for each library
			for (const library of serverLibraries) {
				cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
					JobTypes.LibrarySyncJob,
					JobStatus.Completed,
					generateLibrarySyncJobQueue({
						plexLibraryId: library.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Completed,
						completedAt: new Date().toISOString(),
					}),
				);
			}

			// cy.getCy('sync-server-media-dialog-hide-btn').click();
			// cy.getCy('sync-server-media-dialog').should('not.exist');
		});
	});
});
