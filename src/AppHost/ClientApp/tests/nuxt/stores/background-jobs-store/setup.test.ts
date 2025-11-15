import { describe, beforeAll, test, expect, beforeEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup } from '@services-test-base';
import type { ISetupResult } from '@interfaces';
import { useBackgroundJobsStore } from '@store';

describe('BackgroundJobsStore.setup()', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: 'useBackgroundJobsStore',
		};

		// Act
		const result = subscribeSpyTo(backgroundJobsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
