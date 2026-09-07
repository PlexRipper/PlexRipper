import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { PlexMediaType } from '@dto';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths, PlexLibraryPaths, PlexServerPaths } from '@api/api-paths';
import { generatePlexAccount, generatePlexLibrary, generatePlexServer, generateResultDTO, Seed } from '@mock';
import { useAccountStore, useLibraryStore, useServerStore } from '@store';

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
		const libraries = [
			generatePlexLibrary({
				seed: new Seed(101),
				plexServerId: accessibleServer.id,
				type: PlexMediaType.Movie,
				partialData: { id: 201 },
			}),
			generatePlexLibrary({
				seed: new Seed(102),
				plexServerId: inaccessibleServer.id,
				type: PlexMediaType.Movie,
				partialData: { id: 202 },
			}),
		];
		const accounts = [
			generatePlexAccount({
				id: 1,
				partialData: {
					isEnabled: true,
					plexServerAccess: [accessibleServer.id, inaccessibleServer.id],
					plexLibraryAccess: [libraries[0]!.id],
				},
			}),
		];
		const accountStore = useAccountStore();
		const libraryStore = useLibraryStore();
		const serverStore = useServerStore();
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO(accounts));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO(libraries));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO(servers));

		// Act
		await subscribeSpyTo(accountStore.setup()).onComplete();
		await subscribeSpyTo(serverStore.setup()).onComplete();
		await subscribeSpyTo(libraryStore.setup()).onComplete();

		// Assert
		expect(serverStore.getVisibleServers.map((x) => x.id)).toEqual([accessibleServer.id, inaccessibleServer.id]);
		expect(serverStore.getDisabledServers.map((x) => x.id)).toEqual([disabledServer.id]);
		expect(accountStore.getHasAccountServerAccess(accessibleServer.id)).toBe(true);
		expect(accountStore.getHasAccountServerAccess(inaccessibleServer.id)).toBe(false);
	});
});
