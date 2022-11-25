import { describe, beforeAll, expect, test } from '@jest/globals';
import { take } from 'rxjs/operators';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { GlobalService, LibraryService, ServerService } from '@service';
import { generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import ISetupResult from '@interfaces/service/ISetupResult';
import { generatePlexLibraries } from '@mock/mock-plex-library';
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

		// Act
		const serverSetupResult = subscribeSpyTo(serverSetup$);
		const librarySetupResult = subscribeSpyTo(librarySetup$);
		await serverSetupResult.onComplete();
		await librarySetupResult.onComplete();
		const serverByLibraryId$ = LibraryService.getServerByLibraryId(libraries[5].id).pipe(take(1));
		const serverByLibraryIdResult = subscribeSpyTo(serverByLibraryId$);
		await serverByLibraryIdResult.onComplete();

		// Assert
		expect(serverSetupResult.receivedComplete()).toBe(true);
		expect(librarySetupResult.receivedComplete()).toBe(true);
		const values = serverByLibraryIdResult.getValues();
		expect(values).toHaveLength(1);
		expect(serverByLibraryIdResult.getFirstValue()?.id).toEqual(2);
	});
});
