import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { firstValueFrom, Subject, throwError } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { NotificationPaths } from '@api-urls';
import { notificationApi } from '@api';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { NotificationLevel, type BaseResultDTO, type CountResponseDTO, type NotificationDTO } from '@dto';
import type { ResultDTO } from '@interfaces';
import { useNotificationsStore } from '@store';

const success: BaseResultDTO = { isSuccess: true, statusCode: 200, errors: [], successes: [] };

function notification(id: number): NotificationDTO {
	return { id, level: NotificationLevel.Information, createdAt: '2026-10-19T00:00:00Z', message: `Notification ${id}`, hidden: false };
}

describe('Notification action recovery', () => {
	let { mock } = baseVars();
	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});
	afterEach(() => vi.restoreAllMocks());

	test('Should keep failed hide visible and hide only its row after a successful retry', async () => {
		// Arrange
		const store = useNotificationsStore();
		const rows = [notification(1), notification(2)];
		store.notifications = rows;
		const path = NotificationPaths.setNotificationVisibilityEndpoint();
		mock.onPatch(path).replyOnce(500, generateFailedResultDTO({ statusCode: 500 }))
			.onPatch(path).replyOnce(200, success);

		// Act
		const failed = subscribeSpyTo(store.hideNotification(1));
		expect(store.notifications).toEqual(rows);
		await failed.onComplete();

		// Assert
		expect(failed.getFirstValue()?.isSuccess).toBe(false);
		expect(store.getVisibleNotifications).toEqual(rows);

		// Act
		await subscribeSpyTo(store.hideNotification(1)).onComplete();

		// Assert
		expect(store.notifications).toEqual([{ ...rows[0], hidden: true }, rows[1]]);
		expect(store.getVisibleNotifications).toEqual([rows[1]]);
		expect(mock.history.patch.map((request) => JSON.parse(request.data))).toEqual([
			{ id: 1, hidden: true }, { id: 1, hidden: true },
		]);
	});

	test('Should retain all rows after failed clear and clear them on retry', async () => {
		// Arrange
		const store = useNotificationsStore();
		const rows = [notification(1), notification(2)];
		store.notifications = rows;
		const path = NotificationPaths.clearAllNotificationsEndpoint();
		mock.onDelete(path).replyOnce(500, generateFailedResultDTO({ statusCode: 500 }))
			.onDelete(path).replyOnce(200, generateResultDTO({ count: 2 }));

		// Act
		const failed = subscribeSpyTo(store.clearAllNotifications());
		expect(store.notifications).toEqual(rows);
		await failed.onComplete();

		// Assert
		expect(failed.getFirstValue()?.isSuccess).toBe(false);
		expect(store.notifications).toEqual(rows);

		// Act
		await subscribeSpyTo(store.clearAllNotifications()).onComplete();

		// Assert
		expect(store.notifications).toEqual([]);
		expect(mock.history.delete.map((request) => request.url)).toEqual([path, path]);
	});

	test('Should retain notifications arriving while clear is pending', async () => {
		// Arrange
		const store = useNotificationsStore();
		store.notifications = [notification(1), notification(2)];
		const pending = new Subject<ResultDTO<CountResponseDTO>>();
		vi.spyOn(notificationApi, 'clearAllNotificationsEndpoint').mockReturnValue(pending);
		const result = subscribeSpyTo(store.clearAllNotifications());
		const arrival = notification(3);
		store.setNotification(arrival);

		// Act
		pending.next(generateResultDTO({ count: 2 }));
		pending.complete();
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()?.isSuccess).toBe(true);
		expect(store.notifications).toEqual([arrival]);
		expect(store.getVisibleNotifications).toEqual([arrival]);
	});

	test.each(['hide', 'clear'] as const)('Should retain rows after an observable error and allow %s retry', async (action) => {
		// Arrange
		const store = useNotificationsStore();
		const rows = [notification(1), notification(2)];
		store.notifications = rows;
		const method = action === 'hide' ? 'setNotificationVisibilityEndpoint' : 'clearAllNotificationsEndpoint';
		const failure = vi.spyOn(notificationApi, method).mockReturnValueOnce(throwError(() => new Error('Unavailable')));
		const run = () => action === 'hide' ? store.hideNotification(1) : store.clearAllNotifications();

		// Act
		await expect(firstValueFrom(run())).rejects.toThrow('Unavailable');

		// Assert
		expect(store.notifications).toEqual(rows);
		expect(failure).toHaveBeenCalledTimes(1);
		failure.mockRestore();
		mock.onPatch(NotificationPaths.setNotificationVisibilityEndpoint()).reply(200, success);
		mock.onDelete(NotificationPaths.clearAllNotificationsEndpoint()).reply(200, generateResultDTO({ count: 2 }));

		// Act
		await firstValueFrom(run());

		// Assert
		expect(store.notifications).toEqual(action === 'hide' ? [{ ...rows[0], hidden: true }, rows[1]] : []);
	});
});
