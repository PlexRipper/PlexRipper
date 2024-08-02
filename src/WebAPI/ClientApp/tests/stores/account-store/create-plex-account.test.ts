import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { PlexAccountPaths, PlexLibraryPaths, PlexServerPaths } from '@api-urls';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generatePlexAccount, generatePlexLibrariesFromPlexServers, generatePlexServers, generateResultDTO } from '@mock';
import { useAccountStore, useServerStore } from '#build/imports';

describe('AccountService.createPlexAccount()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should refresh servers when plex account is created successfully', async () => {
		// Arrange
		config = {
			seed: 263,
			plexServerCount: 3,
			plexMovieLibraryCount: 3,
		};
		const plexServers = generatePlexServers({ config });
		const plexLibraries = generatePlexLibrariesFromPlexServers({ plexServers, config });
		const plexAccount = generatePlexAccount({ id: 1, plexServers, plexLibraries, config });

		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint())
			.replyOnce(200, generateResultDTO([]))
			.onGet(PlexServerPaths.getAllPlexServersEndpoint())
			.reply(200, generateResultDTO(plexServers));
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint())
			.replyOnce(200, generateResultDTO([]))
			.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint())
			.reply(200, generateResultDTO([plexAccount]));

		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO(plexLibraries));
		mock.onPost(PlexAccountPaths.createPlexAccountEndpoint()).reply(200, generateResultDTO(plexAccount));
		mock.onGet(PlexAccountPaths.getPlexAccountByIdEndpoint(plexAccount.id)).reply(200, generateResultDTO(plexAccount));

		// Subscriptions
		const accountStore = useAccountStore();
		const serverStore = useServerStore();

		// Act
		await subscribeSpyTo(accountStore.setup()).onComplete();
		await subscribeSpyTo(serverStore.setup()).onComplete();

		const createAccountResult = subscribeSpyTo(accountStore.createPlexAccount(plexAccount));
		await createAccountResult.onComplete();
		const getServersResult = serverStore.getServers();

		// Assert
		expect(createAccountResult.receivedComplete()).toEqual(true);
		expect(createAccountResult.getLastValue()).toEqual(plexAccount);
		expect(getServersResult).toEqual(plexServers);
	});
});
