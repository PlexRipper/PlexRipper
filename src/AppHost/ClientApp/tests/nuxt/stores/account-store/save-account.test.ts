import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths } from '@api-urls';
import { generateFailedResultDTO, generatePlexAccount } from '@mock';
import { useAccountStore } from '@store';

describe('AccountStore account saves', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return the failed create result without refreshing dependent stores', async () => {
		// Arrange
		const accountStore = useAccountStore();
		const account = { ...generatePlexAccount({ id: 99, partialData: { isValidated: true } }), validatedAt: '2026-10-17T00:00:00Z' };
		const failedResult = generateFailedResultDTO({ statusCode: 409 });
		mock.onPost(PlexAccountPaths.createPlexAccountEndpoint()).reply(200, failedResult);

		// Act
		const result = subscribeSpyTo(accountStore.createPlexAccount(account));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(failedResult);
		expect(mock.history.get).toHaveLength(0);
	});

	test('Should return the failed update result without refreshing dependent stores', async () => {
		// Arrange
		const accountStore = useAccountStore();
		const account = generatePlexAccount({ id: 99, partialData: { isValidated: true } });
		const failedResult = generateFailedResultDTO({ statusCode: 422 });
		mock.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint()).reply(200, failedResult);

		// Act
		const result = subscribeSpyTo(accountStore.updatePlexAccount(account));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(failedResult);
		expect(mock.history.get).toHaveLength(0);
	});
});
