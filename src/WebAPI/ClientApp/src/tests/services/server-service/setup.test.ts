import { describe, beforeAll, expect, test } from '@jest/globals';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '~/tests/services/_base/base';
import { ServerService, GlobalService } from '@service';
import { generatePlexServers, generateResultDTO } from '@mock';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('ServerService.setup()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
		GlobalService.initializeState();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(generatePlexServers(config)));
		const setup$ = ServerService.setup(ctx);
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: ServerService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toBe(true);
	});
});
