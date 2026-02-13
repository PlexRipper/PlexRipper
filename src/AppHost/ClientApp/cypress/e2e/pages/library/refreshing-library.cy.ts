import { route } from '@fixtures';
import {
	JobStatus,
	JobTypes,
	type LibrarySyncJobQueueDTO,
	LibrarySyncJobStatus,
	MessageTypes,
	PlexMediaType,
} from '@dto';
import { generateLibrarySyncProgress, generateLibrarySyncJobQueue, generateLibrarySyncProgressItem, generateTimeRemaining } from '@factories';
import { generateResultDTO } from '@mock';
import { PlexLibraryPaths } from '@api/api-paths';

describe('Test the refreshing of a PlexLibrary', () => {
	xit('Should display refreshing of the PlexLibrary when sending the refreshing command', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 100000,
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

		cy.getPageData().then((data) => {
			const movieLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.Movie)!;
			const server = data.plexServers.find((x) => x.id === movieLibrary.plexServerId)!;

			cy.intercept('GET', PlexLibraryPaths.refreshLibraryMediaEndpoint(movieLibrary.id), {
				statusCode: 200,
				body: generateResultDTO(movieLibrary),
			});

			// Verify the refresh button is visible before clicking
			cy.getCy('media-overview-refresh-library-btn').should('be.visible');
			cy.getCy('media-overview-refresh-library-btn').click();

			// Verify the refresh container is NOT yet visible before the job starts
			cy.getCy('refresh-library-container').should('not.exist');

			// === PHASE 1: Start the sync job ===
			cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
				JobTypes.LibrarySyncJob,
				JobStatus.Started,
				generateLibrarySyncJobQueue({
					plexLibraryId: movieLibrary.id,
					plexServerId: server.id,
					status: LibrarySyncJobStatus.Processing,
				}),
			);

			// Verify the refresh container is now visible and the overview bar is hidden
			cy.getCy('refresh-library-container').should('be.visible');
			cy.getCy('media-overview-refresh-library-btn').should('not.exist');

			// === PHASE 2: Send progress updates and verify UI updates ===
			const steps = 10;
			const waitTime = 300;
			const totalItems = movieLibrary.count;

			for (let i = 1; i <= steps; i++) {
				const received = i * (totalItems / steps);
				const timeRemaining = generateTimeRemaining((steps - i) * waitTime);

				const items = [generateLibrarySyncProgressItem(PlexMediaType.Movie, {
					received,
					total: totalItems,
					timeRemaining,
				})];

				const progress = generateLibrarySyncProgress({
					type: PlexMediaType.Movie,
					libraryId: movieLibrary.id,
					received: items.reduce((sum, item) => sum + item.received, 0),
					total: items.reduce((sum, item) => sum + item.total, 0),
					items,
				});

				cy.wait(waitTime).hubPublish(
					'progress',
					MessageTypes.LibraryProgress,
					progress,
				);

				// Verify the container remains visible throughout progress updates
				cy.getCy('refresh-library-container').should('be.visible');

				// Verify the progress table row for Movie media type is displayed
				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Movie}-count`)
					.should('contain.text', `${received}/${totalItems}`);
			}

			// === PHASE 3: Complete the sync job ===
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

			// Verify the refresh container is removed and the overview bar is restored
			cy.getCy('refresh-library-container').should('not.exist');
			cy.getCy('media-overview-refresh-library-btn').should('be.visible');
		});
	});

	it('Should display refreshing of the TvShow PlexLibrary when sending the refreshing command', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexTvShowLibraryCount: 1,
			tvShowCount: 100,
			seasonCount: 50,
			episodeCount: 100,
		})
			.then((data) => {
				const tvShowLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.TvShow);
				if (!tvShowLibrary) {
					throw new Error('TvShow library not found');
				}
				// Visit the page
				cy.visit(route(`/tvshows/${tvShowLibrary.id}`));
				return cy.wrap({
					...data,
					tvShowLibrary,
				});
			})
			.as('tvShowLibrary');
		cy.getPageData().then((data) => {
			const tvShowLibrary = data.plexLibraries.find((x) => x.type === PlexMediaType.TvShow)!;
			const server = data.plexServers.find((x) => x.id === tvShowLibrary.plexServerId)!;

			cy.intercept('GET', PlexLibraryPaths.refreshLibraryMediaEndpoint(tvShowLibrary.id), {
				statusCode: 200,
				body: generateResultDTO(tvShowLibrary),
			});

			// Verify the refresh button is visible before clicking
			cy.getCy('media-overview-refresh-library-btn').should('be.visible');
			cy.getCy('media-overview-refresh-library-btn').click();

			// Verify the refresh container is NOT yet visible before the job starts
			cy.getCy('refresh-library-container').should('not.exist');

			// === PHASE 1: Start the sync job ===
			cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
				JobTypes.LibrarySyncJob,
				JobStatus.Started,
				generateLibrarySyncJobQueue({
					plexLibraryId: tvShowLibrary.id,
					plexServerId: server.id,
					status: LibrarySyncJobStatus.Processing,
				}),
			);

			// Verify the refresh container is now visible and the overview bar is hidden
			cy.getCy('refresh-library-container').should('be.visible');
			cy.getCy('media-overview-refresh-library-btn').should('not.exist');

			// === PHASE 2: Send progress updates - TvShows first, then Seasons, then Episodes ===
			const steps = 10;
			const waitTime = 100;
			const tvShowCount = tvShowLibrary.count;
			const seasonCount = tvShowLibrary.seasonCount;
			const episodeCount = tvShowLibrary.episodeCount;
			const totalCount = tvShowCount * seasonCount * episodeCount;

			// Stage 1: TvShows progress from 0% to 100%, Seasons and Episodes not yet started
			for (let i = 1; i <= steps; i++) {
				const tvShowReceived = i * (tvShowCount / steps);

				const items = [
					generateLibrarySyncProgressItem(PlexMediaType.TvShow, {
						received: tvShowReceived,
						total: tvShowCount,
						timeRemaining: generateTimeRemaining((steps - i) * waitTime),
					}),
					generateLibrarySyncProgressItem(PlexMediaType.Season, {
						received: 0,
						total: seasonCount,
						timeRemaining: generateTimeRemaining(steps * waitTime),
					}),
					generateLibrarySyncProgressItem(PlexMediaType.Episode, {
						received: 0,
						total: episodeCount,
						timeRemaining: generateTimeRemaining(steps * waitTime),
					}),
				];

				cy.wait(waitTime).hubPublish(
					'progress',
					MessageTypes.LibraryProgress,
					generateLibrarySyncProgress({
						type: PlexMediaType.TvShow,
						libraryId: tvShowLibrary.id,
						received: tvShowReceived,
						total: totalCount,
						items,
					}),
				);

				cy.getCy('refresh-library-container').should('be.visible');

				// TvShow row (index 0) counter updates; Season and Episode rows remain at 0
				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.TvShow}-count`)
					.should('contain.text', `${tvShowReceived}/${tvShowCount}`);

				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Season}-count`)
					.should('contain.text', `0/${seasonCount}`);

				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Episode}-count`)
					.should('contain.text', `0/${episodeCount}`);
			}

			// Stage 2: TvShows complete, Seasons progress from 0% to 100%, Episodes not yet started
			for (let i = 1; i <= steps; i++) {
				const seasonReceived = i * (seasonCount / steps);

				const items = [
					generateLibrarySyncProgressItem(PlexMediaType.TvShow, {
						received: tvShowCount,
						total: tvShowCount,
						timeRemaining: '00:00:00',
					}),
					generateLibrarySyncProgressItem(PlexMediaType.Season, {
						received: seasonReceived,
						total: seasonCount,
						timeRemaining: generateTimeRemaining((steps - i) * waitTime),
					}),
					generateLibrarySyncProgressItem(PlexMediaType.Episode, {
						received: 0,
						total: episodeCount,
						timeRemaining: generateTimeRemaining(steps * waitTime),
					}),
				];

				cy.wait(waitTime).hubPublish(
					'progress',
					MessageTypes.LibraryProgress,
					generateLibrarySyncProgress({
						type: PlexMediaType.TvShow,
						libraryId: tvShowLibrary.id,
						received: items.reduce((sum, item) => sum + item.received, 0),
						total: totalCount,
						items,
					}),
				);

				cy.getCy('refresh-library-container').should('be.visible');

				// TvShow row complete; Season row (index 1) counter updates; Episode row remains at 0
				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.TvShow}-count`)
					.should('contain.text', `${tvShowCount}/${tvShowCount}`);

				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Season}-count`)
					.should('contain.text', `${seasonReceived}/${seasonCount}`);

				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Episode}-count`)
					.should('contain.text', `0/${episodeCount}`);
			}

			// Stage 3: TvShows and Seasons complete, Episodes progress from 0% to 100%
			for (let i = 1; i <= steps; i++) {
				const episodeReceived = i * (episodeCount / steps);

				const items = [
					generateLibrarySyncProgressItem(PlexMediaType.TvShow, {
						received: tvShowCount,
						total: tvShowCount,
						timeRemaining: '00:00:00',
					}),
					generateLibrarySyncProgressItem(PlexMediaType.Season, {
						received: seasonCount,
						total: seasonCount,
						timeRemaining: '00:00:00',
					}),
					generateLibrarySyncProgressItem(PlexMediaType.Episode, {
						received: episodeReceived,
						total: episodeCount,
						timeRemaining: generateTimeRemaining((steps - i) * waitTime),
					}),
				];

				cy.wait(waitTime).hubPublish(
					'progress',
					MessageTypes.LibraryProgress,
					generateLibrarySyncProgress({
						type: PlexMediaType.TvShow,
						libraryId: tvShowLibrary.id,
						received: items.reduce((sum, item) => sum + item.received, 0),
						total: totalCount,
						items,
					}),
				);

				cy.getCy('refresh-library-container').should('be.visible');

				// TvShow and Season rows complete; Episode row (index 2) counter updates
				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.TvShow}-count`)
					.should('contain.text', `${tvShowCount}/${tvShowCount}`);

				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Season}-count`)
					.should('contain.text', `${seasonCount}/${seasonCount}`);

				cy.getCy(`library-media-sync-progress-row-${PlexMediaType.Episode}-count`)
					.should('contain.text', `${episodeReceived}/${episodeCount}`);
			}

			// === PHASE 3: Complete the sync job ===
			cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
				JobTypes.LibrarySyncJob,
				JobStatus.Completed,
				generateLibrarySyncJobQueue({
					plexLibraryId: tvShowLibrary.id,
					plexServerId: server.id,
					status: LibrarySyncJobStatus.Completed,
					completedAt: new Date().toISOString(),
				}),
			);

			// Verify the refresh container is removed and the overview bar is restored
			cy.getCy('refresh-library-container').should('not.exist');
			cy.getCy('media-overview-refresh-library-btn').should('be.visible');
		});
	});
});
