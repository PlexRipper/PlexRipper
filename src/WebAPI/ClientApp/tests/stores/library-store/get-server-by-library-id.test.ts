import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generatePlexLibrariesFromPlexServers, generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { useServerStore, useLibraryStore } from '#imports';

describe('LibraryStore.getServerByLibraryId()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return the correct server when given a valid libraryId', async () => {
		// Arrange
		config = {
			seed: 23695,
			plexServerCount: 3,
			plexMovieLibraryCount: 5,
		};
		const serverStore = useServerStore();
		const libraryStore = useLibraryStore();
		const servers = generatePlexServers({ config });
		const libraries = generatePlexLibrariesFromPlexServers({ plexServers: servers, config });

		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO(libraries));

		const testLibrary = libraries[2];
		const testServer = servers.find((x) => x.id === testLibrary.plexServerId);

		// Act
		await subscribeSpyTo(serverStore.setup()).onComplete();
		await subscribeSpyTo(libraryStore.setup()).onComplete();
		const serverByLibraryId = libraryStore.getServerByLibraryId(testLibrary.id);

		// Assert
		expect(serverByLibraryId).toEqual(testServer);
	});
});
