import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { generateResultDTO } from '@mock';
import { NOTIFICATION_RELATIVE_PATH } from '@api-urls';
import type { ISetupResult } from '@interfaces';
import { useNotificationsStore } from '~/store';

describe('NotificationService.setup()', () => {
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
		const notificationsStore = useNotificationsStore();

		mock.onGet(NOTIFICATION_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = notificationsStore.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useNotificationsStore.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
