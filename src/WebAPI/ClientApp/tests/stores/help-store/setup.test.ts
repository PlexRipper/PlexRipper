import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars } from '@services-test-base';
import type { ISetupResult } from '@interfaces';
import { useHelpStore } from '~/store';

describe('HelpStore.setup()', () => {
	baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const helpStore = useHelpStore();

		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useHelpStore.name,
		};

		// Act
		const result = subscribeSpyTo(helpStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
