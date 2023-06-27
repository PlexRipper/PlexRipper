import { acceptHMRUpdate } from 'pinia';
import { take } from 'rxjs/operators';
import { NotificationDTO } from '@dto/mainApi';
import { clearAllNotifications, getNotifications, hideNotification } from '@api/notificationApi';

export const useNotificationsStore = defineStore('NotificationsStore', {
	state: (): { notifications: NotificationDTO[] } => ({
		notifications: [],
	}),
	actions: {
		setup() {
			this.fetchNotifications();
		},
		fetchNotifications() {
			getNotifications().subscribe((notifications) => {
				if (notifications.isSuccess) {
					this.notifications = notifications.value ?? [];
				}
			});
		},
		setNotification(notification: NotificationDTO) {
			this.notifications.push(notification);
		},
		hideNotification(id: number): void {
			const i = this.notifications.findIndex((x) => x.id === id);
			if (i > -1) {
				this.notifications.splice(i, i, { ...this.notifications[i], hidden: true });
			}
			hideNotification(id).subscribe();
		},
		clearAllNotifications(): void {
			this.notifications = [];
			clearAllNotifications().subscribe();
		},
	},
	getters: {
		getNotifications(state): NotificationDTO[] {
			return state.notifications;
		},
		getVisibleNotifications(state): NotificationDTO[] {
			return state.notifications.filter((x) => !x.hidden);
		},
	},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useNotificationsStore, import.meta.hot));
}
