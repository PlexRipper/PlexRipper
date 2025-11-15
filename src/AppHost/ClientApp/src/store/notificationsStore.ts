import { defineStore, acceptHMRUpdate } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import { switchMap, tap } from 'rxjs/operators';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import type { NotificationDTO } from '@dto';
import type { ISetupResult } from '@interfaces';
import { notificationApi } from '@api';
import { cloneDeep } from 'lodash-es';

interface INotificationStoreState {
	notifications: NotificationDTO[];
}

export const useNotificationsStore = defineStore('NotificationsStore', () => {
	const defaultState: INotificationStoreState = {
		notifications: [],
	};

	const state = reactive<INotificationStoreState>(cloneDeep(defaultState));

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.fetchNotifications().pipe(
				switchMap(() =>
					of({
						name: 'useNotificationsStore',
						isSuccess: true,
					}),
				),
			);
		},
		fetchNotifications() {
			return notificationApi.getAllNotificationsEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess) {
						state.notifications = result.value ?? [];
					}
				}),
			);
		},
		setNotification(notification: NotificationDTO) {
			state.notifications.push(notification);
		},
		hideNotification(id: number): void {
			const i = state.notifications.findIndex((x) => x.id === id);
			if (i > -1) {
				const current = state.notifications[i]!;
				state.notifications.splice(i, 1, { ...current, hidden: true });
			}
			notificationApi
				.setNotificationVisibilityEndpoint({
					id,
					hidden: true,
				})
				.subscribe();
		},
		clearAllNotifications(): void {
			state.notifications = [];
			notificationApi.clearAllNotificationsEndpoint().subscribe();
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {
		getNotifications: computed((): NotificationDTO[] => state.notifications),
		getVisibleNotifications: computed((): NotificationDTO[] => state.notifications.filter((x) => !x.hidden)),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useNotificationsStore, import.meta.hot));
}
