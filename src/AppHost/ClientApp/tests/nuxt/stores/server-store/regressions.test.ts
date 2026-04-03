import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO, generatePlexServers, generateResultDTO } from '@mock';
import { PlexServerPaths } from '@api-urls';
import { useServerStore } from '@store';

describe('ServerStore regressions', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should not refresh settings when setting a server alias fails', async () => {
		// Arrange
		const serverStore = useServerStore();
		mock.onGet('/api/PlexServer/1/set-server-alias').reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(serverStore.setServerAlias(1, 'Updated Alias'));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.filter((request) => request.url === '/api/Settings')).toHaveLength(0);
	});

	test('Should not refresh settings when hiding a server fails', async () => {
		// Arrange
		const serverStore = useServerStore();
		mock.onGet('/api/PlexServer/1/set-server-hidden').reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(serverStore.setServerHidden(1, true));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.filter((request) => request.url === '/api/Settings')).toHaveLength(0);
	});

	test('Should refresh the server after pausing downloads', async () => {
		// Arrange
		config = { plexServerCount: 1 };
		const serverStore = useServerStore();
		const initialServer = generatePlexServers({ config })[0]!;
		const updatedServer = generatePlexServers({ config })[0]!;
		updatedServer.id = initialServer.id;
		updatedServer.name = 'Paused server name';
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([initialServer]));
		mock.onPut(PlexServerPaths.pausePlexServerDownloadsEndpoint(initialServer.id)).reply(200, { isSuccess: true, statusCode: 200, errors: [], successes: [] });
		mock.onGet(PlexServerPaths.getPlexServerByIdEndpoint(initialServer.id)).reply(200, generateResultDTO(updatedServer));
		await subscribeSpyTo(serverStore.refreshPlexServers()).onComplete();

		// Act
		await subscribeSpyTo(serverStore.setServerPaused(initialServer.id, true)).onComplete();

		// Assert
		expect(serverStore.getServer(initialServer.id)?.name).toBe('Paused server name');
	});

	test('Should refresh the server after resuming downloads', async () => {
		// Arrange
		config = { plexServerCount: 1 };
		const serverStore = useServerStore();
		const initialServer = generatePlexServers({ config })[0]!;
		const updatedServer = generatePlexServers({ config })[0]!;
		updatedServer.id = initialServer.id;
		updatedServer.name = 'Resumed server name';
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([initialServer]));
		mock.onPut(PlexServerPaths.resumePlexServerDownloadsEndpoint(initialServer.id)).reply(200, { isSuccess: true, statusCode: 200, errors: [], successes: [] });
		mock.onGet(PlexServerPaths.getPlexServerByIdEndpoint(initialServer.id)).reply(200, generateResultDTO(updatedServer));
		await subscribeSpyTo(serverStore.refreshPlexServers()).onComplete();

		// Act
		await subscribeSpyTo(serverStore.setServerPaused(initialServer.id, false)).onComplete();

		// Assert
		expect(serverStore.getServer(initialServer.id)?.name).toBe('Resumed server name');
	});
});
