import { describe, beforeAll, test, expect, beforeEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useBackgroundJobsStore } from '@store';
import { BackgroundJobsPaths } from '@api/generated/BackgroundJobs';
import { generateFailedResultDTO } from '@mock';

describe('BackgroundJobsStore.setup()', () => {
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
		const backgroundJobsStore = useBackgroundJobsStore();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.BackgroundJobsStore,
		};

		// Act
		const result = subscribeSpyTo(backgroundJobsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});

	test('Should report failure when fetching background jobs fails', async () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();
		mock.onGet(BackgroundJobsPaths.getAllBackgroundJobsEndpoint()).reply(500, generateFailedResultDTO());
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.BackgroundJobsStore,
		};

		// Act
		const result = subscribeSpyTo(backgroundJobsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
