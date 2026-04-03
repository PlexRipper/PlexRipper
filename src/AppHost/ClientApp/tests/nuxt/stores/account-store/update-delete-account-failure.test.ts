import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generateFailedResultDTO,
	generatePlexAccount,
	generatePlexLibrariesFromPlexServers,
	generatePlexServers,
	generateResultDTO,
	Seed,
} from '@mock';
import { PlexAccountPaths, PlexLibraryPaths, PlexServerPaths } from '@api-urls';
import { useAccountStore } from '@store';

describe('AccountStore failure contracts', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return failed result and not trigger refresh calls when updating account fails', async () => {
		// Arrange
		config = {
			seed: 263,
			plexServerCount: 2,
			plexMovieLibraryCount: 2,
		};
		const seed = new Seed(config.seed!);
		const plexServers = generatePlexServers({ config });
		const plexLibraries = generatePlexLibrariesFromPlexServers({ seed, plexServers, config });
		const account = generatePlexAccount({ id: 55, plexServers, plexLibraries, config });

		mock.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint()).reply(500, generateFailedResultDTO());

		const accountStore = useAccountStore();

		// Act
		const result = subscribeSpyTo(accountStore.updatePlexAccount(account));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.find((x) => x.url === PlexAccountPaths.getAllPlexAccountsEndpoint())).toBeUndefined();
		expect(mock.history.get.find((x) => x.url === PlexServerPaths.getAllPlexServersEndpoint())).toBeUndefined();
		expect(mock.history.get.find((x) => x.url === PlexLibraryPaths.getAllPlexLibrariesEndpoint())).toBeUndefined();
	});

	test('Should return failed result and not refresh accounts when deleting account fails', async () => {
		// Arrange
		const accountId = 56;
		mock.onDelete(PlexAccountPaths.deletePlexAccountByIdEndpoint(accountId)).reply(500, generateFailedResultDTO());

		const accountStore = useAccountStore();

		// Act
		const result = subscribeSpyTo(accountStore.deleteAccount(accountId));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toMatchObject({ isSuccess: false });
		expect(mock.history.get.find((x) => x.url === PlexAccountPaths.getAllPlexAccountsEndpoint())).toBeUndefined();
	});

	test('Should still refresh related state and return account when update succeeds', async () => {
		// Arrange
		config = {
			seed: 264,
			plexServerCount: 2,
			plexMovieLibraryCount: 2,
		};
		const seed = new Seed(config.seed!);
		const plexServers = generatePlexServers({ config });
		const plexLibraries = generatePlexLibrariesFromPlexServers({ seed, plexServers, config });
		const account = generatePlexAccount({ id: 57, plexServers, plexLibraries, config });

		mock.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint()).reply(200, generateResultDTO(account));
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([account]));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO(plexServers));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO(plexLibraries));

		const accountStore = useAccountStore();

		// Act
		const result = subscribeSpyTo(accountStore.updatePlexAccount(account));
		await result.onComplete();

		// Assert
		expect(result.getLastValue()).toEqual(account);
		expect(mock.history.get.find((x) => x.url === PlexAccountPaths.getAllPlexAccountsEndpoint())).toBeDefined();
		expect(mock.history.get.find((x) => x.url === PlexServerPaths.getAllPlexServersEndpoint())).toBeDefined();
		expect(mock.history.get.find((x) => x.url === PlexLibraryPaths.getAllPlexLibrariesEndpoint())).toBeDefined();
	});
});
