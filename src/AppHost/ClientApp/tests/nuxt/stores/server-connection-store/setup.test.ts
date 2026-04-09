import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { PlexServerConnectionPaths } from '@api-urls';
import { generateFailedResultDTO } from '@mock';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useServerConnectionStore } from '@store';

describe('ServerConnectionStore.setup() failure handling', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should report failure when refreshing server connections fails', async () => {
		// Arrange
		const serverConnectionStore = useServerConnectionStore();
		mock.onGet(PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint()).reply(500, generateFailedResultDTO());
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.ServerConnectionStore,
		};

		// Act
		const result = subscribeSpyTo(serverConnectionStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
