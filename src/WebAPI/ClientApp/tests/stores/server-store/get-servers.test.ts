import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
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

	test('Should return all servers when servers are set in the store', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		const serverStore = useServerStore();
		const servers = generatePlexServers({ config });
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));

		// Act
		await subscribeSpyTo(serverStore.setup()).onComplete();
		const serversResult = serverStore.getServers();

		// Assert
		expect(serversResult).toEqual(servers);
	});

	test('Should return all servers by id when servers are set in the store', async () => {
		// Arrange
		config = {
			seed: 4587,
			plexServerCount: 6,
		};
		const serverStore = useServerStore();
		const servers = generatePlexServers({ config });
		const serverIds = servers.map((x) => x.id);
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));

		// Act
		await subscribeSpyTo(serverStore.setup()).onComplete();
		const serversResult = serverStore.getServers(serverIds.slice(0, 3));

		// Assert
		const firstValue = serversResult;
		expect(firstValue.length).toEqual(3);
		for (let i = 0; i < 3; i++) {
			expect(firstValue[i].id).toEqual(serverIds[i]);
		}
	});
});
