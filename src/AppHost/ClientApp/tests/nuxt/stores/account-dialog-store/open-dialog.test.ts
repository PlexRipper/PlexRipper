import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generatePlexAccount, generatePlexLibrariesFromPlexServers, generatePlexServers, Seed } from '@mock';
import { useAccountDialogStore, useAccountStore } from '@store';

describe('AccountDialogStore.openDialog()', () => {
	let { mock: _mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		_mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should load existing account data when editing an account', () => {
		// Arrange
		config = {
			seed: 263,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
		};
		const seed = new Seed(config.seed!);
		const plexServers = generatePlexServers({ config });
		const plexLibraries = generatePlexLibrariesFromPlexServers({ seed, plexServers, config });
		const account = generatePlexAccount({
			id: 42,
			plexServers,
			plexLibraries,
			config,
		});

		const accountStore = useAccountStore();
		accountStore.accounts = [account];
		const accountDialogStore = useAccountDialogStore();

		// Act
		accountDialogStore.openDialog({ accountId: account.id });

		// Assert
		expect(accountDialogStore.isNewAccount).toEqual(false);
		expect(accountDialogStore.id).toEqual(account.id);
		expect(accountDialogStore.username).toEqual(account.username);
		expect(accountDialogStore.displayName).toEqual(account.displayName);
	});

	test('Should reset dialog fields when opening a new account after editing an existing one', () => {
		// Arrange
		config = {
			seed: 263,
			plexServerCount: 1,
			plexMovieLibraryCount: 1,
		};
		const seed = new Seed(config.seed!);
		const plexServers = generatePlexServers({ config });
		const plexLibraries = generatePlexLibrariesFromPlexServers({ seed, plexServers, config });
		const account = generatePlexAccount({
			id: 7,
			plexServers,
			plexLibraries,
			config,
		});

		const accountStore = useAccountStore();
		accountStore.accounts = [account];
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: account.id });

		// Act
		accountDialogStore.openDialog({ accountId: 0 });

		// Assert
		expect(accountDialogStore.isNewAccount).toEqual(true);
		expect(accountDialogStore.id).toEqual(0);
		expect(accountDialogStore.username).toEqual('');
		expect(accountDialogStore.displayName).toEqual('');
		expect(accountDialogStore.email).toEqual('');
		expect(accountDialogStore.password).toEqual('');
	});
});
