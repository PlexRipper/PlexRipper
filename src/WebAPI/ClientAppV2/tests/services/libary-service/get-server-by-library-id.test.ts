import { beforeAll, describe, expect, test } from '@jest/globals';
import { take } from 'rxjs/operators';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { LibraryService, ServerService } from '@service';
import { generateResultDTO } from '@mock';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { generatePlexServersAndLibraries } from '@mock/mock-combination';

describe('LibraryService.getServerByLibraryId()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return the correct server when given a valid libraryId', async () => {
		// Arrange
		config = {
			seed: 23695,
			plexServerCount: 3,
			plexLibraryCount: 20,
		};

		const { servers, libraries } = generatePlexServersAndLibraries(config);

		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO(libraries));
		const serverSetup$ = ServerService.setup(ctx);
		const librarySetup$ = LibraryService.setup(ctx);
		const testLibrary = libraries[5];
		// Act
		const serverSetupResult = subscribeSpyTo(serverSetup$);
		const librarySetupResult = subscribeSpyTo(librarySetup$);
		await serverSetupResult.onComplete();
		await librarySetupResult.onComplete();
		const serverByLibraryId$ = LibraryService.getServerByLibraryId(testLibrary.id).pipe(take(1));
		const serverByLibraryIdResult = subscribeSpyTo(serverByLibraryId$);
		await serverByLibraryIdResult.onComplete();

		// Assert
		expect(serverSetupResult.receivedComplete()).toBe(true);
		expect(librarySetupResult.receivedComplete()).toBe(true);
		const values = serverByLibraryIdResult.getValues();
		expect(values).toHaveLength(1);
		expect(serverByLibraryIdResult.getFirstValue()?.id).toEqual(testLibrary.plexServerId);
	});
});
