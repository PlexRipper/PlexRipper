import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars } from '@services-test-base';
import type { ISetupResult } from '@interfaces';
import { useAlertStore } from '#build/imports';

describe('AlertStore.setup()', () => {
	baseVars();

	beforeAll(() => {
		baseSetup();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const alertStore = useAlertStore();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useAlertStore.name,
		};

		// Act
		const result = subscribeSpyTo(alertStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).to.equal(true);
	});
});
