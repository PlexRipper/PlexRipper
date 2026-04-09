import { describe, beforeAll, beforeEach, afterEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateFailedResultDTO, generateResultDTO, generateSettingsModel } from '@mock';
import { StoreNames, type ISetupResult } from '@interfaces';
import { SettingsPaths } from '@api-urls';
import { useSettingsStore } from '@store';

describe('SettingsStore.setup()', () => {
	// eslint-disable-next-line prefer-const
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.useFakeTimers();
	});

	afterEach(() => {
		vi.useRealTimers();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		mock.onGet(SettingsPaths.getUserSettingsEndpoint()).reply(200, generateResultDTO(generateSettingsModel({ config })));
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.SettingsStore,
		};

		// Act

		const result = subscribeSpyTo(settingsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});

	test('Should report failure when refreshing settings fails', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		mock.onGet(SettingsPaths.getUserSettingsEndpoint()).reply(500, generateFailedResultDTO());
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.SettingsStore,
		};

		// Act
		const result = subscribeSpyTo(settingsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
	});

	test('Should not initialize autosave when refreshing settings fails', async () => {
		// Arrange
		const settingsStore = useSettingsStore();
		mock.onGet(SettingsPaths.getUserSettingsEndpoint()).reply(500, generateFailedResultDTO());
		mock.onPut(SettingsPaths.updateUserSettingsEndpoint()).reply(200, generateFailedResultDTO());
		await subscribeSpyTo(settingsStore.setup()).onComplete();

		// Act
		settingsStore.generalSettings.firstTimeSetup = false;
		await vi.advanceTimersByTimeAsync(600);

		// Assert
		expect(mock.history.put.filter((request) => request.url === SettingsPaths.updateUserSettingsEndpoint())).toHaveLength(0);
	});
});
