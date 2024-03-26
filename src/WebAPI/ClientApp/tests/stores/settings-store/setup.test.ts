import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateResultDTO, generateSettingsModel } from '@mock';
import type { ISetupResult } from '@interfaces';
import { SETTINGS_RELATIVE_PATH } from '@api-urls';
import { useSettingsStore } from '#imports';

describe('SettingsStore.setup()', () => {
	// eslint-disable-next-line prefer-const
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		mock.onGet(SETTINGS_RELATIVE_PATH).reply(200, generateResultDTO(generateSettingsModel({ config })));
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useSettingsStore.name,
		};

		// Act

		const result = subscribeSpyTo(settingsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
