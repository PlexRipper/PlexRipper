import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { LibrarySyncJobStatus, PlexConnectionTypes, type PlexServerConnectionDTO, type PlexServerStatusDTO } from '@dto';
import { PlexLibraryPaths } from '@api/api-paths';
import { generateFailedResultDTO, generateLibrarySyncJobQueue, generateResultDTO } from '@mock';
import { useLibraryStore, useServerConnectionStore } from '@store';
import { plexLibraryApi } from '@api';
import { throwError } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';

describe('LibraryStore queue state', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	test('Should expose queued and processing rows separately by library and server', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const queued = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Queued });
		const processing = generateLibrarySyncJobQueue({ plexLibraryId: 12, plexServerId: 1, status: LibrarySyncJobStatus.Processing });
		const otherServerProcessing = generateLibrarySyncJobQueue({ plexLibraryId: 21, plexServerId: 2, status: LibrarySyncJobStatus.Processing });
		libraryStore.syncQueues = [queued, processing, otherServerProcessing];

		// Act
		const queuedResult = libraryStore.getLibrarySyncQueue(11);
		const processingResult = libraryStore.getProcessingLibrarySyncQueue(1);

		// Assert
		expect(queuedResult).toEqual(queued);
		expect(processingResult).toEqual(processing);
		expect(libraryStore.getIsLibrarySyncing(11)).toBe(false);
		expect(libraryStore.getIsLibrarySyncing(12)).toBe(true);
		expect(libraryStore.getProcessingLibrarySyncQueue(2)).toEqual(otherServerProcessing);
	});

	test('Should report a same-server processing blocker even when connectivity is unknown', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		libraryStore.syncQueues = [
			generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Queued }),
			generateLibrarySyncJobQueue({ plexLibraryId: 12, plexServerId: 1, status: LibrarySyncJobStatus.Processing }),
		];

		// Act
		const result = libraryStore.getLibrarySyncWaitingReason(11);

		// Assert
		expect(result).toBe('busy');
	});

	test('Should report offline before a same-server blocker when the current connection is known offline', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const serverConnectionStore = useServerConnectionStore();
		libraryStore.syncQueues = [
			generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Queued }),
			generateLibrarySyncJobQueue({ plexLibraryId: 12, plexServerId: 1, status: LibrarySyncJobStatus.Processing }),
		];
		serverConnectionStore.serverConnections = [createConnection(1, false)];

		// Act
		const result = libraryStore.getLibrarySyncWaitingReason(11);

		// Assert
		expect(result).toBe('offline');
	});

	test('Should use waiting when connectivity is unknown and no same-server processor is visible', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		libraryStore.syncQueues = [
			generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Queued, isServerOffline: true }),
		];

		// Act
		const result = libraryStore.getLibrarySyncWaitingReason(11);

		// Assert
		expect(result).toBe('waiting');
	});

	test('Should let current online connectivity override a stale offline queue flag', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const serverConnectionStore = useServerConnectionStore();
		libraryStore.syncQueues = [
			generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Queued, isServerOffline: true }),
		];
		serverConnectionStore.serverConnections = [createConnection(1, true)];

		// Act
		const result = libraryStore.getLibrarySyncWaitingReason(11);

		// Assert
		expect(result).toBe('waiting');
	});

	test('Should apply the authoritative refresh queue result without a second status lookup', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const queue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Processing });
		mock.onPost(PlexLibraryPaths.refreshLibraryMediaEndpoint(11)).reply(200, generateResultDTO(queue));

		// Act
		const result = subscribeSpyTo(libraryStore.reSyncLibrary(11, true));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(queue);
		expect(libraryStore.getLibrarySyncQueue(11)).toEqual(queue);
		expect(mock.history.get.filter((request) => request.url === PlexLibraryPaths.getLibrarySyncStatusEndpoint())).toHaveLength(0);
	});

	test('Should retain known queues when refresh admission is rejected', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const existingQueue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Processing });
		libraryStore.syncQueues = [existingQueue];
		mock.onPost(PlexLibraryPaths.refreshLibraryMediaEndpoint(11)).reply(400, generateFailedResultDTO());

		// Act
		const result = subscribeSpyTo(libraryStore.reSyncLibrary(11));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toBeNull();
		expect(libraryStore.syncQueues).toEqual([existingQueue]);
	});

	test('Should preserve known queues after an unsuccessful status snapshot', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const existingQueue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Processing });
		libraryStore.syncQueues = [existingQueue];
		mock.onGet(PlexLibraryPaths.getLibrarySyncStatusEndpoint()).reply(500, generateFailedResultDTO());

		// Act
		const result = subscribeSpyTo(libraryStore.refreshLibrarySyncStatus());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual([existingQueue]);
		expect(libraryStore.syncQueues).toEqual([existingQueue]);
	});

	test('Should preserve known queues after a status transport error', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const existingQueue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Processing });
		libraryStore.syncQueues = [existingQueue];
		vi.spyOn(plexLibraryApi, 'getLibrarySyncStatusEndpoint').mockImplementation(() => throwError(() => new Error('transport')));

		// Act
		const result = subscribeSpyTo(libraryStore.refreshLibrarySyncStatus());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual([existingQueue]);
		expect(libraryStore.syncQueues).toEqual([existingQueue]);
	});

	test('Should clear known queues after a successful empty status snapshot', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		libraryStore.syncQueues = [generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1 })];
		mock.onGet(PlexLibraryPaths.getLibrarySyncStatusEndpoint()).reply(200, generateResultDTO([]));

		// Act
		const result = subscribeSpyTo(libraryStore.refreshLibrarySyncStatus());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual([]);
		expect(libraryStore.syncQueues).toEqual([]);
	});

	test('Should refresh the authoritative queue after successful cancellation and preserve the cancellation result', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const queue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Cancelled });
		mock.onDelete(PlexLibraryPaths.cancelLibrarySyncEndpoint(11)).reply(200, generateResultDTO(undefined));
		mock.onGet(PlexLibraryPaths.getLibrarySyncStatusEndpoint()).reply(200, generateResultDTO([queue]));

		// Act
		const result = subscribeSpyTo(libraryStore.cancelLibrarySync(11));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()?.isSuccess).toBe(true);
		expect(libraryStore.getLibrarySyncQueue(11)).toEqual(queue);
		expect(mock.history.get.filter((request) => request.url === PlexLibraryPaths.getLibrarySyncStatusEndpoint())).toHaveLength(1);
	});

	test('Should leave queues unchanged when cancellation fails', async () => {
		// Arrange
		const libraryStore = useLibraryStore();
		const existingQueue = generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Processing });
		libraryStore.syncQueues = [existingQueue];
		mock.onDelete(PlexLibraryPaths.cancelLibrarySyncEndpoint(11)).reply(400, generateFailedResultDTO());

		// Act
		const result = subscribeSpyTo(libraryStore.cancelLibrarySync(11));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()?.isSuccess).toBe(false);
		expect(libraryStore.syncQueues).toEqual([existingQueue]);
		expect(mock.history.get.filter((request) => request.url === PlexLibraryPaths.getLibrarySyncStatusEndpoint())).toHaveLength(0);
	});
});

function createConnection(plexServerId: number, isSuccessful: boolean): PlexServerConnectionDTO {
	return {
		id: plexServerId,
		plexServerId,
		address: '127.0.0.1',
		port: 32400,
		url: 'http://127.0.0.1:32400',
		protocol: 'http',
		type: PlexConnectionTypes.Public,
		chosenConnection: true,
		isCustom: false,
		isPlexTvConnection: false,
		local: false,
		relay: false,
		iPv4: true,
		iPv6: false,
		latestConnectionStatus: {
			id: plexServerId,
			plexServerId,
			plexServerConnectionId: plexServerId,
			isSuccessful,
			lastChecked: new Date().toISOString(),
			statusCode: isSuccessful ? 200 : 500,
			statusMessage: isSuccessful ? 'Completed' : 'Failed',
		} as PlexServerStatusDTO,
	};
}
