import { take } from 'rxjs/operators';
import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import LibraryService from '@service/libraryService';
import ServerService from '@service/serverService';
import { generatePlexLibraries, generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { PlexMediaType } from '@dto/mainApi';

describe('LibraryService.getServerByLibraryId()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return the correct server when given a valid libraryId', async () => {
		// Arrange
		config = {
			seed: 23695,
			plexServerCount: 3,
			plexMovieLibraryCount: 5,
		};

		const servers = generatePlexServers({ config });
		const libraries = servers
			.map((x) => generatePlexLibraries({ plexServerId: x.id, type: PlexMediaType.Movie, config }))
			.flat();

		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO(libraries));
		const serverSetup$ = ServerService.setup();
		const librarySetup$ = LibraryService.setup();
		const testLibrary = libraries[2];
		// Act
		const serverSetupResult = subscribeSpyTo(serverSetup$);
		const librarySetupResult = subscribeSpyTo(librarySetup$);
		await serverSetupResult.onComplete();
		await librarySetupResult.onComplete();
		const serverByLibraryId$ = LibraryService.getServerByLibraryId(testLibrary.id).pipe(take(1));
		const serverByLibraryIdResult = subscribeSpyTo(serverByLibraryId$);
		await serverByLibraryIdResult.onComplete();

		// Assert
		expect(serverSetupResult.receivedComplete()).equals(true);
		expect(librarySetupResult.receivedComplete()).equals(true);
		const values = serverByLibraryIdResult.getValues();
		expect(values).lengthOf(1);
		expect(serverByLibraryIdResult.getFirstValue()?.id).equals(testLibrary.plexServerId);
	});
});
