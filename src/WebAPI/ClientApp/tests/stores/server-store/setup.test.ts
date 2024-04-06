import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { PlexServerPaths } from '@api/api-paths';
import { generatePlexServers, generateResultDTO } from '@mock';
import type { ISetupResult } from '@interfaces';
import { useServerStore } from '#imports';

describe('ServerStore.setup()', () => {
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
		config = {
			plexServerCount: 3,
		};

		const serverStore = useServerStore();
		const plexServers = generatePlexServers({ config });
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO(plexServers));
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useServerStore.name,
		};

		// Act
		const result = subscribeSpyTo(serverStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
