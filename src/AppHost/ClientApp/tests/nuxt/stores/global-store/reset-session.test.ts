import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateLibrarySyncJobQueue } from '@mock';
import { IntegrationProvisioningState, IntegrationType, TestConnectionStatus } from '@dto';
import {
	useAccountStore,
	useAuthenticationStore,
	useGlobalStore,
	useIntegrationStore,
	useLibraryStore,
	useSignalrStore,
} from '@store';

describe('GlobalStore.resetSession()', () => {
	let { mock: _mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		_mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	test('Should clear every session store when SignalR teardown fails', async () => {
		// Arrange
		const globalStore = useGlobalStore();
		const signalrStore = useSignalrStore();
		const accountStore = useAccountStore();
		const authenticationStore = useAuthenticationStore();
		const libraryStore = useLibraryStore();
		const integrationStore = useIntegrationStore();
		accountStore.accounts = [{ id: 1 } as (typeof accountStore.accounts)[number]];
		authenticationStore.isLoggedIn = true;
		libraryStore.syncQueues = [generateLibrarySyncJobQueue({ plexLibraryId: 1, plexServerId: 1 })];
		integrationStore.items = [{
			id: 'integration-1',
			name: 'Sonarr',
			type: IntegrationType.Sonarr,
			baseUrl: 'http://sonarr',
			category: 'reaparr-sonarr',
			downloadFolderId: 1,
			externalDownloadClientId: null,
			externalIndexerId: null,
			lastConnectionTestErrorMessage: null,
			lastConnectionTestHttpStatusCode: null,
			lastConnectionTestStatus: TestConnectionStatus.Unknown,
			lastConnectionTestedAt: null,
			provisioningState: IntegrationProvisioningState.Unconfigured,
		}];
		vi.spyOn(signalrStore, 'resetSession').mockRejectedValue(new Error('Hub stop failed'));

		// Act
		await globalStore.resetSession();

		// Assert
		expect(signalrStore.resetSession).toHaveBeenCalledOnce();
		expect(accountStore.accounts).toEqual([]);
		expect(authenticationStore.isLoggedIn).toBe(false);
		expect(libraryStore.syncQueues).toEqual([]);
		expect(integrationStore.items).toEqual([]);
	});
});
