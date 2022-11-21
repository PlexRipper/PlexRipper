import { describe, beforeAll, expect, test } from '@jest/globals';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { GlobalService } from '@service';
import {
	DOWNLOAD_RELATIVE_PATH,
	FOLDER_PATH_RELATIVE_PATH,
	NOTIFICATION_RELATIVE_PATH,
	PLEX_ACCOUNT_RELATIVE_PATH,
	PLEX_LIBRARY_RELATIVE_PATH,
	PLEX_SERVER_RELATIVE_PATH,
	SETTINGS_RELATIVE_PATH,
} from '@api-urls';
import { generatePlexServers, generateResultDTO, generateSettings } from '@mock';

describe('GlobalService.getConfigReady()', () => {
	let { ctx, mock, config } = baseVars();
	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when setup is run', async () => {
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
		const configReady$ = GlobalService.getConfigReady();

		// Act
		const setupResult = subscribeSpyTo(setup$);
		const configResult = subscribeSpyTo(configReady$);
		await setupResult.onComplete();
		// Assert
		expect(setupResult.receivedComplete()).toBe(true);
		expect(configResult.getFirstValue()).not.toBeNaN();
		expect(configResult.getFirstValue().version).not.toBeFalsy();
	});
});
