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
					plexServerAccess: [accessibleServer.id],
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

	test('Should retain server access before any libraries are discovered', async () => {
		// Arrange
		const serverId = 101;
		const account = generatePlexAccount({
			id: 1,
			config: { seed: 101 },
			partialData: {
				isEnabled: true,
				plexServerAccess: [serverId],
				plexLibraryAccess: [],
			},
		});
		const accountStore = useAccountStore();
		const libraryStore = useLibraryStore();
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([account]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));

		// Act
		await subscribeSpyTo(accountStore.setup()).onComplete();
		await subscribeSpyTo(libraryStore.setup()).onComplete();

		// Assert
		expect(libraryStore.getAllLibrariesByServerId(serverId)).toEqual([]);
		expect(accountStore.getHasAccountServerAccess(serverId)).toBe(true);
	});

	test.each([
		{
			scenario: 'an enabled account has server access but no library access',
			isEnabled: true,
			plexServerAccess: [101],
			plexLibraryAccess: [],
			expectedServerAccess: true,
			expectedLibraryAccess: false,
		},
		{
			scenario: 'only a disabled account has server and library access',
			isEnabled: false,
			plexServerAccess: [101],
			plexLibraryAccess: [201],
			expectedServerAccess: false,
			expectedLibraryAccess: false,
		},
		{
			scenario: 'an enabled account has library access but no server access',
			isEnabled: true,
			plexServerAccess: [],
			plexLibraryAccess: [201],
			expectedServerAccess: false,
			expectedLibraryAccess: true,
		},
	])('Should distinguish server and library permissions when $scenario', async ({
		isEnabled,
		plexServerAccess,
		plexLibraryAccess,
		expectedServerAccess,
		expectedLibraryAccess,
	}) => {
		// Arrange
		const library = generatePlexLibrary({
			seed: new Seed(101),
			plexServerId: 101,
			type: PlexMediaType.Movie,
			partialData: { id: 201 },
		});
		const account = generatePlexAccount({
			id: 1,
			config: { seed: 101 },
			partialData: { isEnabled, plexServerAccess, plexLibraryAccess },
		});
		const accountStore = useAccountStore();
		const libraryStore = useLibraryStore();
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([account]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([library]));

		// Act
		await subscribeSpyTo(accountStore.setup()).onComplete();
		await subscribeSpyTo(libraryStore.setup()).onComplete();

		// Assert
		expect(accountStore.getHasAccountServerAccess(101)).toBe(expectedServerAccess);
		expect(accountStore.getHasAccountLibraryAccess(library.id)).toBe(expectedLibraryAccess);
	});
});
