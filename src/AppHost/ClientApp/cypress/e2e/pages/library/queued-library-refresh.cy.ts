import { route } from '@fixtures';
import {
	JobStatus,
	JobTypes,
	LibrarySyncJobStatus,
	MessageTypes,
	PlexMediaType,
	type LibrarySyncJobQueueDTO,
} from '@dto';
import {
	generateLibrarySyncJobQueue,
	generateLibrarySyncProgress,
	generateLibrarySyncProgressItem,
} from '@factories';
import { generateResultDTO } from '@mock';
import { PlexLibraryPaths, PlexServerConnectionPaths } from '@api/api-paths';

describe('Queued library refresh visibility', () => {
	beforeEach(() => {
		cy.hubClear();
		// The mock package's commands otherwise retain the previous document's window.
		Cypress.Commands.overwrite('hubPublish', (_original, hubName, messageType, payload) =>
			cy.window().then((autWindow) => {
				const hub = autWindow['cypress-signalr-mock']?.mocks.find((mock) => mock.name === hubName);
				if (!hub) {
					throw new Error(`SignalR mock hub ${hubName} is not ready in the current page`);
				}
				hub.publish(messageType, payload);
			}),
		);
	});

	it('keeps previously synced media visible while B waits behind A and then reloads only after completion', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 2,
			movieCount: 20,
		})
			.then((data) => {
				const libraries = data.plexLibraries.filter((library) => library.type === PlexMediaType.Movie);
				const processingLibrary = libraries[0];
				const queuedLibrary = libraries[1];
				if (!processingLibrary || !queuedLibrary) {
					throw new Error('Two movie libraries are required');
				}
				const server = data.plexServers.find((item) => item.id === queuedLibrary.plexServerId);
				if (!server) {
					throw new Error('Plex server not found');
				}
				let queues = data.plexLibrarySyncJobStatuses.map((queue) => queue.plexLibraryId === processingLibrary.id
					? { ...queue, status: LibrarySyncJobStatus.Processing, completedAt: null }
					: queue);
				let mediaRequestCount = 0;
				let mediaRequestsBeforeTerminal = 0;
				cy.intercept('GET', PlexLibraryPaths.getLibrarySyncStatusEndpoint(), (request) => {
					request.reply(200, generateResultDTO(queues));
				}).as('queueSnapshot');
				cy.intercept('GET', /\/api\/PlexMedia\?/, () => {
					mediaRequestCount++;
				}).as('libraryMedia');
				cy.intercept('GET', PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint(), {
					statusCode: 200,
					body: generateResultDTO(data.plexServerConnections.map((connection) => ({
						...connection,
						latestConnectionStatus: {
							...connection.latestConnectionStatus,
							isSuccessful: true,
						},
					}))),
				});

				cy.visit(route(`/movies/${queuedLibrary.id}`));
				cy.getCy('page-load-completed').should('exist');
				cy.wait('@libraryMedia');
				cy.getCy('poster-table').should('exist');
				cy.getCy('change-view-mode-btn').click();
				cy.getCy('view-mode-table-btn').click();
				cy.getCy('media-table').should('be.visible');

				cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
					JobTypes.LibrarySyncJob,
					JobStatus.Started,
					generateLibrarySyncJobQueue({
						plexLibraryId: processingLibrary.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Processing,
					}),
				);

				cy.intercept('POST', PlexLibraryPaths.refreshLibraryMediaEndpoint(queuedLibrary.id), (request) => {
					const queue = generateLibrarySyncJobQueue({
						plexLibraryId: queuedLibrary.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Queued,
					});
					queues = queues.map((item) => item.plexLibraryId === queue.plexLibraryId ? queue : item);
					request.reply(200, generateResultDTO(queue));
				}).as('queueLibraryRefresh');

				cy.getCy('media-overview-refresh-library-btn').click();
				cy.getCy('refresh-mode-incremental-button').click();
				cy.wait('@queueLibraryRefresh').its('request.body').should('deep.equal', {
					forceLibrarySync: true,
					forceMediaRefresh: false,
				});

				cy.getCy('media-overview-library-sync-queued-banner')
					.should('be.visible')
					.and('contain.text', 'Refresh queued');
				cy.getCy('media-overview-library-sync-queued-message')
					.should('contain.text', `Waiting for ${processingLibrary.title} to finish refreshing on ${server.name}.`);
				cy.getCy('media-table').should('be.visible');
				cy.getCy('media-overview-refresh-library-btn').should('be.disabled');

				// A fresh document restores the queued snapshot and registers new mock connections.
				cy.hubClear();
				cy.visit(route(`/movies/${queuedLibrary.id}`));
				cy.getCy('page-load-completed').should('exist');
				cy.wait('@queueSnapshot');
				cy.getCy('media-overview-library-sync-queued-banner').should('be.visible');
				cy.wait('@libraryMedia');
				cy.get('.media-overview-results').should('be.visible');
				cy.then(() => {
					mediaRequestsBeforeTerminal = mediaRequestCount;
				});
				// A stale 100% progress event must not turn queued B into a completed/progress view.
				cy.hubPublish(
					'progress',
					MessageTypes.LibraryProgress,
					generateLibrarySyncProgress({
						type: PlexMediaType.Movie,
						libraryId: queuedLibrary.id,
						received: queuedLibrary.count,
						total: queuedLibrary.count,
						items: [generateLibrarySyncProgressItem(PlexMediaType.Movie, {
							received: queuedLibrary.count,
							total: queuedLibrary.count,
						})],
					}),
				);
				cy.getCy('media-overview-library-sync-queued-banner').should('be.visible');
				cy.getCy('refresh-library-container').should('not.exist');

				cy.getCy('background-activity-button').click();
				cy.getCy(`${JobTypes.LibrarySyncJob}activity-button`).click();
				cy.getCy('sync-server-media-dialog').should('be.visible');
				cy.getCy('sync-server-media-dialog-library-status')
					.should('contain.text', 'Waiting for')
					.and('not.contain.text', '%');
				cy.getCy('sync-server-media-dialog-library-eta').should('not.exist');
				cy.getCy('sync-server-media-dialog-hide-btn').click();
				cy.then(() => {
					const queue = queues.find((item) => item.plexLibraryId === processingLibrary.id)!;
					queue.status = LibrarySyncJobStatus.Completed;
					queue.completedAt = new Date().toISOString();
					cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(JobTypes.LibrarySyncJob, JobStatus.Completed, queue);
				});
				cy.then(() => {
					const queue = queues.find((item) => item.plexLibraryId === queuedLibrary.id)!;
					queue.status = LibrarySyncJobStatus.Processing;
					queue.completedAt = null;
					cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(JobTypes.LibrarySyncJob, JobStatus.Started, queue);
				});
				cy.getCy('refresh-library-container').should('be.visible');
				cy.then(() => {
					const queue = queues.find((item) => item.plexLibraryId === queuedLibrary.id)!;
					queue.status = LibrarySyncJobStatus.Failed;
					queue.errorMessage = 'Plex refresh failed';
					cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(JobTypes.LibrarySyncJob, JobStatus.Completed, queue);
				});
				cy.get('.media-overview-results').should('be.visible');
				cy.then(() => {
					expect(mediaRequestCount, 'failure must not reload media as success').to.equal(mediaRequestsBeforeTerminal);
				});

				// Retry through the ordinary refresh action, then observe completion without Processing.
				cy.getCy('media-overview-refresh-library-btn').click();
				cy.getCy('refresh-mode-incremental-button').click();
				cy.wait('@queueLibraryRefresh');
				cy.getCy('media-overview-library-sync-queued-banner').should('be.visible');

				cy.then(() => {
					const queue = queues.find((item) => item.plexLibraryId === queuedLibrary.id)!;
					queue.status = LibrarySyncJobStatus.Completed;
					queue.completedAt = new Date().toISOString();
					cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(JobTypes.LibrarySyncJob, JobStatus.Completed, queue);
				});
				cy.getCy('media-overview-library-sync-queued-banner').should('not.exist');
				cy.getCy('media-overview-refresh-library-btn').should('be.visible');
				cy.get('.media-overview-results').should('be.visible');
				cy.wait('@libraryMedia');
				cy.then(() => {
					expect(mediaRequestCount, 'only completion reloads media').to.equal(mediaRequestsBeforeTerminal + 1);
				});
			});
	});

	it('shows a first-sync queued panel and removes it after an actual cancellation without a success reload', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 2,
			movieCount: 20,
		})
			.then((data) => {
				const libraries = data.plexLibraries.filter((library) => library.type === PlexMediaType.Movie);
				const processingLibrary = libraries[0];
				const queuedLibrary = libraries[1];
				if (!processingLibrary || !queuedLibrary) {
					throw new Error('Two movie libraries are required');
				}
				const server = data.plexServers.find((item) => item.id === queuedLibrary.plexServerId);
				if (!server) {
					throw new Error('Plex server not found');
				}
				let queues = data.plexLibrarySyncJobStatuses.map((queue) => queue.plexLibraryId === processingLibrary.id
					? { ...queue, status: LibrarySyncJobStatus.Processing, completedAt: null }
					: queue);
				cy.intercept('GET', PlexLibraryPaths.getLibrarySyncStatusEndpoint(), (request) => {
					request.reply(200, generateResultDTO(queues));
				}).as('queueSnapshot');
				cy.intercept('GET', PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint(), {
					statusCode: 200,
					body: generateResultDTO(data.plexServerConnections.map((connection) => ({
						...connection,
						latestConnectionStatus: {
							...connection.latestConnectionStatus,
							isSuccessful: true,
						},
					}))),
				});

				cy.intercept('GET', PlexLibraryPaths.getAllPlexLibrariesEndpoint(), {
					statusCode: 200,
					body: generateResultDTO(data.plexLibraries.map((library) => library.id === queuedLibrary.id
						? { ...library, syncedAt: null, count: 0 }
						: library)),
				});
				cy.intercept('GET', PlexLibraryPaths.getPlexLibraryByIdEndpoint(queuedLibrary.id), {
					statusCode: 200,
					body: generateResultDTO({ ...queuedLibrary, syncedAt: null, count: 0 }),
				});
				cy.visit(route(`/movies/${queuedLibrary.id}`));
				cy.getCy('page-load-completed').should('exist');
				cy.contains('The library media data has not yet been synced').should('exist');

				cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
					JobTypes.LibrarySyncJob,
					JobStatus.Started,
					generateLibrarySyncJobQueue({
						plexLibraryId: processingLibrary.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Processing,
					}),
				);
				cy.intercept('POST', PlexLibraryPaths.refreshLibraryMediaEndpoint(queuedLibrary.id), (request) => {
					const queue = generateLibrarySyncJobQueue({
						plexLibraryId: queuedLibrary.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Queued,
					});
					queues = queues.map((item) => item.plexLibraryId === queue.plexLibraryId ? queue : item);
					request.reply(200, generateResultDTO(queue));
				}).as('queueFirstSync');
				cy.intercept('DELETE', PlexLibraryPaths.cancelLibrarySyncEndpoint(queuedLibrary.id), (request) => {
					queues = queues.map((queue) => queue.plexLibraryId === queuedLibrary.id
						? { ...queue, status: LibrarySyncJobStatus.Cancelled }
						: queue);
					request.reply(200, generateResultDTO(undefined));
				}).as('cancelQueuedRefresh');

				cy.getCy('media-overview-refresh-library-btn').click();
				cy.getCy('refresh-mode-incremental-button').click();
				cy.wait('@queueFirstSync').its('request.body').should('deep.equal', {
					forceLibrarySync: true,
					forceMediaRefresh: false,
				});
				cy.getCy('media-overview-library-sync-queued-panel').should('be.visible');
				cy.contains('The library media data has not yet been synced').should('not.exist');
				cy.getCy('cancel-queued-library-sync-button').click();
				cy.wait('@cancelQueuedRefresh').its('request.method').should('equal', 'DELETE');
				cy.wait('@queueSnapshot');

				cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(
					JobTypes.LibrarySyncJob,
					JobStatus.Completed,
					generateLibrarySyncJobQueue({
						plexLibraryId: queuedLibrary.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Cancelled,
					}),
				);
				cy.getCy('media-overview-library-sync-queued-panel').should('not.exist');
				cy.contains('The library media data has not yet been synced').should('exist');
				cy.getCy('background-activity-button-badge').should('contain.text', 1);
				cy.then(() => {
					const queue = queues.find((item) => item.plexLibraryId === processingLibrary.id)!;
					queue.status = LibrarySyncJobStatus.Completed;
					queue.completedAt = new Date().toISOString();
					cy.hubPublishJobStatusUpdate<LibrarySyncJobQueueDTO>(JobTypes.LibrarySyncJob, JobStatus.Completed, queue);
				});
				cy.getCy('background-activity-button-badge').should('not.exist');
				cy.getCy('media-overview-library-sync-queued-panel').should('not.exist');
				cy.contains('The library media data has not yet been synced').should('exist');
			});
	});

	it('keeps disabled-library restrictions primary while an offline queued refresh is shown', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
			movieCount: 10,
		})
			.then((data) => {
				const library = data.plexLibraries.find((item) => item.type === PlexMediaType.Movie);
				if (!library) {
					throw new Error('Movie library not found');
				}
				const server = data.plexServers.find((item) => item.id === library.plexServerId);
				if (!server) {
					throw new Error('Plex server not found');
				}
				cy.intercept('GET', PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint(), {
					statusCode: 200,
					body: generateResultDTO(data.plexServerConnections.map((connection) => ({
						...connection,
						latestConnectionStatus: {
							...connection.latestConnectionStatus,
							isSuccessful: false,
						},
					}))),
				});

				cy.intercept('GET', PlexLibraryPaths.getAllPlexLibrariesEndpoint(), {
					statusCode: 200,
					body: generateResultDTO(data.plexLibraries.map((item) => item.id === library.id
						? { ...item, isEnabled: false }
						: item)),
				});
				cy.intercept('GET', PlexLibraryPaths.getPlexLibraryByIdEndpoint(library.id), {
					statusCode: 200,
					body: generateResultDTO({ ...library, isEnabled: false }),
				});
				cy.visit(route(`/movies/${library.id}`));
				cy.getCy('page-load-completed').should('exist');
				cy.contains(`The library "${library.title}" is disabled`).should('exist');
				cy.intercept('POST', PlexLibraryPaths.refreshLibraryMediaEndpoint(library.id), {
					statusCode: 200,
					body: generateResultDTO(generateLibrarySyncJobQueue({
						plexLibraryId: library.id,
						plexServerId: server.id,
						status: LibrarySyncJobStatus.Queued,
						isServerOffline: true,
					})),
				});

				cy.getCy('media-overview-refresh-library-btn').click();
				cy.getCy('refresh-mode-incremental-button').click();
				cy.getCy('media-overview-library-sync-queued-banner')
					.should('contain.text', 'Refresh queued')
					.and('contain.text', 'Waiting for the server to come online.');
				cy.contains(`The library "${library.title}" is disabled`).should('exist');
			});
	});
});
