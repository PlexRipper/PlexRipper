import { describe, beforeAll, expect, test } from '@jest/globals';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { GlobalService, SettingsService } from '@service';
import { generateResultDTO, generateSettings } from '@mock';
import ISetupResult from '@interfaces/service/ISetupResult';
import { SETTINGS_RELATIVE_PATH } from '@api-urls';

describe('SettingsService.setup()', () => {
	// eslint-disable-next-line prefer-const
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
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: SettingsService.name,
		};
		const setup$ = SettingsService.setup(ctx);

		// Act
		mock.onGet(SETTINGS_RELATIVE_PATH).reply(200, generateResultDTO(generateSettings(config)));
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toBe(true);
	});
});
