import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { throwError } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths } from '@api-urls';
import { plexAccountApi } from '@api';
import { generateFailedResultDTO, generatePlexAccount, generateResultDTO } from '@mock';
import { useAccountStore } from '@store';

describe('AccountStore committed deletion', () => {
	let { mock } = baseVars();

	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});
	afterEach(() => vi.restoreAllMocks());

	test.each([200, 500])('Should retain the DELETE result and remove only its account when refresh returns %i', async (status) => {
		// Arrange
		const store = useAccountStore();
		const target = generatePlexAccount({ id: 9 });
		const retained = generatePlexAccount({ id: 10 });
		store.accounts = [target, retained];
		const deleted = generateResultDTO(true);
		mock.onDelete(PlexAccountPaths.deletePlexAccountByIdEndpoint(target.id)).reply(200, deleted);
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(status,
			status === 200 ? generateResultDTO([retained]) : generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(store.deleteAccount(target.id));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(deleted);
		expect(store.accounts).toEqual([retained]);
		expect(mock.history.delete.map((request) => request.url)).toEqual([PlexAccountPaths.deletePlexAccountByIdEndpoint(9)]);
		expect(mock.history.get.map((request) => request.url)).toEqual([PlexAccountPaths.getAllPlexAccountsEndpoint()]);
	});

	test('Should preserve every account and skip refresh when DELETE fails', async () => {
		// Arrange
		const store = useAccountStore();
		const accounts = [generatePlexAccount({ id: 9 }), generatePlexAccount({ id: 10 })];
		store.accounts = accounts;
		const failed = generateFailedResultDTO({ statusCode: 409 });
		mock.onDelete(PlexAccountPaths.deletePlexAccountByIdEndpoint(9)).reply(200, failed);

		// Act
		const result = subscribeSpyTo(store.deleteAccount(9));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(failed);
		expect(store.accounts).toEqual(accounts);
		expect(mock.history.get).toEqual([]);
		expect(mock.history.delete.map((request) => request.url)).toEqual([PlexAccountPaths.deletePlexAccountByIdEndpoint(9)]);
	});

	test('Should preserve committed deletion when the refresh observable errors', async () => {
		// Arrange
		const store = useAccountStore();
		const retained = generatePlexAccount({ id: 10 });
		store.accounts = [generatePlexAccount({ id: 9 }), retained];
		const deleted = generateResultDTO(true);
		mock.onDelete(PlexAccountPaths.deletePlexAccountByIdEndpoint(9)).reply(200, deleted);
		const refresh = vi.spyOn(plexAccountApi, 'getAllPlexAccountsEndpoint')
			.mockReturnValue(throwError(() => new Error('Refresh unavailable')));

		// Act
		const result = subscribeSpyTo(store.deleteAccount(9));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(deleted);
		expect(store.accounts).toEqual([retained]);
		expect(refresh).toHaveBeenCalledTimes(1);
	});
});
