import { describe, beforeAll, beforeEach, expect, test } from 'vitest';
import ServerService from '@service/serverService';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { generatePlexServers, generateResultDTO } from '@mock';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('ServerService.setup()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(generatePlexServers({ config })));
		const setup$ = ServerService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: ServerService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
