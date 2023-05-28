import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import SettingsService from '@service/settingsService';
import { generateResultDTO, generateSettingsModel } from '@mock';
import ISetupResult from '@interfaces/service/ISetupResult';
import { SETTINGS_RELATIVE_PATH } from '@api-urls';

describe('SettingsService.setup()', () => {
	// eslint-disable-next-line prefer-const
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
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
		const setup$ = SettingsService.setup();

		// Act
		mock.onGet(SETTINGS_RELATIVE_PATH).reply(200, generateResultDTO(generateSettingsModel({ config })));
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
