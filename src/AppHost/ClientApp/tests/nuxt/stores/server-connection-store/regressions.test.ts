import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO, generatePlexServerConnection } from '@mock';
import { PlexServerConnectionPaths, PlexServerPaths } from '@api-urls';
import { useServerConnectionStore } from '@store';

describe('ServerConnectionStore regressions', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should not refresh server connections when checking server status fails', async () => {
		// Arrange
		const serverConnectionStore = useServerConnectionStore();
		serverConnectionStore.serverConnections = [generatePlexServerConnection({ id: 1, plexServerId: 7 })];
		mock.onGet(PlexServerConnectionPaths.checkAllConnectionsStatusByPlexServerEndpoint(7)).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(serverConnectionStore.checkServerStatus(7));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(serverConnectionStore.getConnectionLoading(1)).toBe(false);
		expect(mock.history.get.filter((request) => request.url === PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint())).toHaveLength(0);
	});

	test('Should not refresh the server when setting the preferred connection fails', async () => {
		// Arrange
		const serverConnectionStore = useServerConnectionStore();
		mock.onGet(PlexServerPaths.setPreferredPlexServerConnectionEndpoint(7, 3)).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(serverConnectionStore.setPreferredPlexServerConnection(7, 3));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.filter((request) => request.url === PlexServerPaths.getPlexServerByIdEndpoint(7))).toHaveLength(0);
	});
});
