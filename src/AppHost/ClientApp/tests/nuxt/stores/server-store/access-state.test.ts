import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths, PlexServerPaths } from '@api/api-paths';
import { generatePlexAccount, generatePlexServer, generateResultDTO } from '@mock';
import { useAccountStore, useServerStore } from '@store';

describe('ServerStore access state getters', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return enabled inaccessible servers as visible but not disabled', async () => {
		// Arrange
		const accessibleServer = generatePlexServer({ id: 101, partialData: { isEnabled: true, name: 'Accessible' } });
		const inaccessibleServer = generatePlexServer({ id: 102, partialData: { isEnabled: true, name: 'Inaccessible' } });
		const disabledServer = generatePlexServer({ id: 103, partialData: { isEnabled: false, name: 'Disabled' } });
		const servers = [accessibleServer, inaccessibleServer, disabledServer];
		const accounts = [
			generatePlexAccount({
				id: 1,
				partialData: {
					isEnabled: true,
					plexServerAccess: [accessibleServer.id],
					plexLibraryAccess: [],
				},
			}),
		];
		const accountStore = useAccountStore();
		const serverStore = useServerStore();
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO(accounts));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO(servers));

		// Act
		await subscribeSpyTo(accountStore.setup()).onComplete();
		await subscribeSpyTo(serverStore.setup()).onComplete();

		// Assert
		expect(serverStore.getVisibleServers.map((x) => x.id)).toEqual([accessibleServer.id, inaccessibleServer.id]);
		expect(serverStore.getDisabledServers.map((x) => x.id)).toEqual([disabledServer.id]);
		expect(accountStore.getHasAccountServerAccess(accessibleServer.id)).toBe(true);
		expect(accountStore.getHasAccountServerAccess(inaccessibleServer.id)).toBe(false);
	});
});
