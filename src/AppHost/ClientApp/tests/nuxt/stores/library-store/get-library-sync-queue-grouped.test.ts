import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { LibrarySyncJobStatus } from '@dto';
import { generateLibrarySyncJobQueue } from '@mock';
import { useLibraryStore } from '@store';
import { baseSetup } from '@services-test-base';

describe('LibraryStore.getLibrarySyncQueueGrouped()', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
	});

	test('Should only return queues for the requested Plex server', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		libraryStore.syncQueues = [
			generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1, status: LibrarySyncJobStatus.Processing }),
			generateLibrarySyncJobQueue({ plexLibraryId: 12, plexServerId: 1 }),
			generateLibrarySyncJobQueue({ plexLibraryId: 21, plexServerId: 2 }),
		];

		// Act
		const result = libraryStore.getLibrarySyncQueueGrouped(1);

		// Assert
		expect(result).toHaveLength(1);
		expect(result[0]?.serverId).toBe(1);
		expect(result[0]?.progress.map((queue) => queue.plexLibraryId)).toEqual([11, 12]);
	});

	test('Should return queues for all Plex servers when no server is requested', () => {
		// Arrange
		const libraryStore = useLibraryStore();
		libraryStore.syncQueues = [
			generateLibrarySyncJobQueue({ plexLibraryId: 11, plexServerId: 1 }),
			generateLibrarySyncJobQueue({ plexLibraryId: 21, plexServerId: 2 }),
		];

		// Act
		const result = libraryStore.getLibrarySyncQueueGrouped();

		// Assert
		expect(result.map((server) => server.serverId)).toEqual([1, 2]);
	});
});
