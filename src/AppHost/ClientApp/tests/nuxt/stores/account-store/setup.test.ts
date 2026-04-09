import { describe, beforeAll, test, expect, beforeEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { PlexAccountPaths } from '@api/api-paths';
import { StoreNames, type ISetupResult } from '@interfaces';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { useAccountStore } from '@store';

describe('AccountStore.setup()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const accountStore = useAccountStore();
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([]));
		const setup$ = accountStore.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.AccountStore,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});

	test('Should report failure when refreshing accounts fails', async () => {
		// Arrange
		const accountStore = useAccountStore();
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(500, generateFailedResultDTO());
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.AccountStore,
		};

		// Act
		const result = subscribeSpyTo(accountStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
