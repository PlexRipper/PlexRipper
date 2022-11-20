import { expect, test } from '@jest/globals';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { GlobalService } from '@service';
import { generatePlexServers, generateResultDTO, generateSettings } from '@mock';
import {
	DOWNLOAD_RELATIVE_PATH,
	FOLDER_PATH_RELATIVE_PATH,
	NOTIFICATION_RELATIVE_PATH,
	PLEX_ACCOUNT_RELATIVE_PATH,
	PLEX_LIBRARY_RELATIVE_PATH,
	PLEX_SERVER_RELATIVE_PATH,
	SETTINGS_RELATIVE_PATH,
} from '@api-urls';

describe('GlobalService.setup()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when all setup services are run', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		mock.onGet(DOWNLOAD_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(FOLDER_PATH_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(NOTIFICATION_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(generatePlexServers(config)));
		mock.onGet(SETTINGS_RELATIVE_PATH).reply(200, generateResultDTO(generateSettings(config)));

		const setup$ = GlobalService.setupObservable(ctx);

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.receivedComplete()).toBe(true);
	});
});
