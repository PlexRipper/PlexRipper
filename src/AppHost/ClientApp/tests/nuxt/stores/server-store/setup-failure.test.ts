import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { PlexServerPaths } from '@api-urls';
import { generateFailedResultDTO } from '@mock';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useServerStore } from '@store';

describe('ServerStore.setup() failure handling', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should report failure when refreshing servers fails', async () => {
		// Arrange
		const serverStore = useServerStore();
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(500, generateFailedResultDTO());
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.ServerStore,
		};

		// Act
		const result = subscribeSpyTo(serverStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
