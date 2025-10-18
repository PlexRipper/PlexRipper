import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { PlexAccountPaths, PlexLibraryPaths, PlexServerPaths } from '@api-urls';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexAccount,
	generatePlexLibrariesFromPlexServers,
	generatePlexServers,
	generateResultDTO, Seed,
} from '@mock';
import { useAccountStore, useServerStore } from '@store';

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
		const seed = new Seed(config.seed!);

		const plexServers = generatePlexServers({ config });
		const plexLibraries = generatePlexLibrariesFromPlexServers({ seed, plexServers, config });
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

		const createAccountResult = subscribeSpyTo(accountStore.createPlexAccount({
			authenticationToken: plexAccount.authenticationToken,
			customAuthenticationToken: plexAccount.customAuthenticationToken,
			clientId: plexAccount.clientId,
			displayName: plexAccount.displayName,
			email: plexAccount.email,
			is2Fa: plexAccount.is2Fa,
			isEnabled: plexAccount.isEnabled,
			isMain: plexAccount.isMain,
			isValidated: plexAccount.isValidated,
			validatedAt: plexAccount.validatedAt!,
			password: plexAccount.password,
			plexId: plexAccount.plexId,
			title: plexAccount.title,
			username: plexAccount.username,
			uuid: plexAccount.uuid,

		}));
		await createAccountResult.onComplete();
		const getServersResult = serverStore.getServers();

		// Assert
		expect(createAccountResult.receivedComplete()).toEqual(true);
		expect(createAccountResult.getLastValue()).toEqual(plexAccount);
		expect(getServersResult).toEqual(plexServers);
	});
});
