import { describe, beforeAll, test, expect, beforeEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { useSignalrStore } from '@store';
import { subscribeSpyTo, baseSetup, baseVars } from '~~/tests/_base/base';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('SignalrStore.setup()', () => {
	const { appConfig } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const signalrStore = useSignalrStore();

		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useSignalrStore.name,
		};

		// Act
		const result = subscribeSpyTo(signalrStore.setup(appConfig));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
