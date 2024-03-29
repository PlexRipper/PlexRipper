import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup } from '~~/tests/_base/base';
import type { ISetupResult } from '@interfaces';
import { useMediaStore } from '#imports';

describe('MediaStore.setup()', () => {
	beforeAll(() => {
		baseSetup();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const mediaStore = useMediaStore();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useMediaStore.name,
		};

		// Act
		const result = subscribeSpyTo(mediaStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
