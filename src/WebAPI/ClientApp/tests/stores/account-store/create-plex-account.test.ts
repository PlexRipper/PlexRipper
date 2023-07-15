import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generatePlexAccount, generatePlexLibrariesFromPlexServers, generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_ACCOUNT_RELATIVE_PATH, PLEX_LIBRARY_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
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

		mock.onGet(PLEX_SERVER_RELATIVE_PATH)
			.replyOnce(200, generateResultDTO([]))
			.onGet(PLEX_SERVER_RELATIVE_PATH)
			.reply(200, generateResultDTO(plexServers));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH)
			.replyOnce(200, generateResultDTO([]))
			.onGet(PLEX_ACCOUNT_RELATIVE_PATH)
			.reply(200, generateResultDTO([plexAccount]));

		mock.onGet(PLEX_LIBRARY_RELATIVE_PATH).reply(200, generateResultDTO(plexLibraries));
		mock.onPost(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO(plexAccount));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH + `/${plexAccount.id}`).reply(200, generateResultDTO(plexAccount));

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
