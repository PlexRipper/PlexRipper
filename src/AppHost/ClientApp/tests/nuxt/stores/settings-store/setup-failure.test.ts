import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO } from '@mock';
import { StoreNames, type ISetupResult } from '@interfaces';
import { SettingsPaths } from '@api-urls';
import { useSettingsStore } from '@store';

describe('SettingsStore.setup() failure handling', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		vi.useFakeTimers();
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	afterEach(() => {
		vi.useRealTimers();
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
