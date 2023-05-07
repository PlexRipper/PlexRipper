import { expect, test } from 'vitest';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import GlobalService from '@service/globalService';
import { generatePlexServers, generateResultDTO, generateSettingsModel } from '@mock';
import {
	DOWNLOAD_RELATIVE_PATH,
	FOLDER_PATH_RELATIVE_PATH,
	NOTIFICATION_RELATIVE_PATH,
	PLEX_ACCOUNT_RELATIVE_PATH,
	PLEX_LIBRARY_RELATIVE_PATH,
	PLEX_SERVER_CONNECTION_RELATIVE_PATH,
	PLEX_SERVER_RELATIVE_PATH,
	SETTINGS_RELATIVE_PATH,
} from '@api-urls';

describe('GlobalService.setup()', () => {
	let { appConfig, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		appConfig = result.appConfig;
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
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(generatePlexServers({ config })));
		mock.onGet(SETTINGS_RELATIVE_PATH).reply(200, generateResultDTO(generateSettingsModel({ config })));
		mock.onGet(PLEX_SERVER_CONNECTION_RELATIVE_PATH).reply(200, generateResultDTO([]));

		const setup$ = GlobalService.setup(appConfig);

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.receivedComplete()).toEqual(true);
	});
});
