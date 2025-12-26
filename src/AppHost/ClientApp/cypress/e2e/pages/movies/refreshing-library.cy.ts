import { route } from '@fixtures';
import {
	JobStatus,
	JobTypes,
	type LibrarySyncJobQueueDTO,
	LibrarySyncJobStatus,
	MessageTypes,
	PlexMediaType,
} from '@dto';
import { generateLibraryProgress, generateLibrarySyncJobQueue } from '@factories';
import { generateResultDTO } from '@mock';
import { PlexLibraryPaths } from '@api/api-paths';

describe('Test the refreshing of a PlexLibrary', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 1000,
		})
			.then((data) => {
				const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie);
				if (!movieLibrary) {
					throw new Error('Movie library not found');
				}
				// Visit the page
				cy.visit(route(`/movies/${movieLibrary.id}`));
				return cy.wrap({
					...data,
					movieLibrary,
				});
			})
			.as('movieLibrary');
	});

	it('Should display refreshing of the PlexLibrary when sending the refreshing command', () => {
		cy.getPageData().then((data) => {
			const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie)!;
			const server = data.plexServers.find((x) => x.id === movieLibrary.plexServerId)!;

			cy.intercept('GET', PlexLibraryPaths.refreshLibraryMediaEndpoint(movieLibrary.id), {
				statusCode: 200,
				body: generateResultDTO(movieLibrary),
			});
			cy.getCy(`media-overview-refresh-library-btn`).click();

			// Start the sync job - this adds an entry to syncQueues with Processing status
			cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
				JobTypes.LibrarySyncJob,
				JobStatus.Started,
				generateLibrarySyncJobQueue({
					plexLibraryId: movieLibrary.id,
					plexServerId: server.id,
					status: LibrarySyncJobStatus.Processing,
				}),
			);

			// Verify the refresh container is visible
			cy.getCy('refresh-library-container').should('be.visible');

			// Send progress updates
			for (let i = 0; i < 5; i++) {
				cy.wait(500).hubPublish(
					'progress',
					MessageTypes.LibraryProgress,
					generateLibraryProgress({
						libraryId: movieLibrary.id,
						received: i * 25,
						total: 100,
					}),
				);
				cy.getCy('refresh-library-container').should('be.visible');
			}

			// Complete the sync job
			cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
				JobTypes.LibrarySyncJob,
				JobStatus.Completed,
				generateLibrarySyncJobQueue({
					plexLibraryId: movieLibrary.id,
					plexServerId: server.id,
					status: LibrarySyncJobStatus.Completed,
					completedAt: new Date().toISOString(),
				}),
			);

			cy.getCy('refresh-library-container').should('not.exist');
		});
	});
});
