import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PLEX_SERVER_CONNECTION_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { generatePlexServers, generateResultDTO } from '@mock';
import { useServerStore } from '~/store';

describe('ServerStore.getServers()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return a server by Id when given a serverId', async () => {
		// Arrange
		config = {
			seed: 1536,
			plexServerCount: 3,
		};
		const serverStore = useServerStore();
		const servers = generatePlexServers({ config });
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_SERVER_CONNECTION_RELATIVE_PATH).reply(200, generateResultDTO([]));

		// Act
		await subscribeSpyTo(serverStore.setup()).onComplete();
		const serverResult = serverStore.getServer(servers[2].id);

		// Assert
		expect(serverResult).not.toBeNaN();
		expect(serverResult?.id).toEqual(servers[2].id);
		expect(serverResult).toEqual(servers[2]);
	});
});
