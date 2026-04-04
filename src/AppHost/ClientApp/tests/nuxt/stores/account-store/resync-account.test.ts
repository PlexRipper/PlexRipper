import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { useAccountStore } from '@store';
import { PlexLibraryPaths } from '@/types/api/generated/PlexLibrary';
import { PlexServerPaths } from '@/types/api/generated/PlexServer';

describe('AccountStore.reSyncAccount()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should refresh dependent stores when account access refresh succeeds', async () => {
		// Arrange
		const accountStore = useAccountStore();
		mock.onGet(PlexAccountPaths.refreshPlexAccountAccessEndpoint(7)).reply(200, generateResultDTO([]));
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));

		// Act
		const result = subscribeSpyTo(accountStore.reSyncAccount(7));
		await result.onComplete();

		// Assert
		expect(mock.history.get.filter((request) => request.url === PlexAccountPaths.getAllPlexAccountsEndpoint())).toHaveLength(1);
		expect(mock.history.get.filter((request) => request.url === PlexServerPaths.getAllPlexServersEndpoint())).toHaveLength(1);
		expect(mock.history.get.filter((request) => request.url === PlexLibraryPaths.getAllPlexLibrariesEndpoint())).toHaveLength(1);
		expect(accountStore.accessSyncLoading).toBe(false);
		expect(result.getLastValue()).toMatchObject({ isSuccess: true });
	});

	test('Should not refresh dependent stores when account access refresh fails', async () => {
		// Arrange
		const accountStore = useAccountStore();
		mock.onGet(PlexAccountPaths.refreshPlexAccountAccessEndpoint(7)).reply(500, generateFailedResultDTO());
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));

		// Act
		const result = subscribeSpyTo(accountStore.reSyncAccount(7));
		await result.onComplete();

		// Assert
		expect(mock.history.get.filter((request) => request.url === PlexAccountPaths.getAllPlexAccountsEndpoint())).toHaveLength(0);
		expect(mock.history.get.filter((request) => request.url === PlexServerPaths.getAllPlexServersEndpoint())).toHaveLength(0);
		expect(mock.history.get.filter((request) => request.url === PlexLibraryPaths.getAllPlexLibrariesEndpoint())).toHaveLength(0);
		expect(accountStore.accessSyncLoading).toBe(false);
		expect(result.getLastValue()).toMatchObject({ isSuccess: false });
	});

	test('Should reset loading when account access refresh errors', async () => {
		// Arrange
		const accountStore = useAccountStore();
		mock.onGet(PlexAccountPaths.refreshPlexAccountAccessEndpoint(7)).networkError();

		// Act
		const result = subscribeSpyTo(accountStore.reSyncAccount(7));
		await result.onComplete();

		// Assert
		expect(accountStore.accessSyncLoading).toBe(false);
		expect(result.getLastValue()).toMatchObject({ isSuccess: false });
	});
});
