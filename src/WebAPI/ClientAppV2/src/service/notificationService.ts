import { Observable, of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';

import { NotificationDTO } from '@dto/mainApi';
import { BaseService } from '@service';
import { clearAllNotifications, getNotifications, hideNotification } from '@api/notificationApi';
import IStoreState from '@interfaces/service/IStoreState';
import ISetupResult from '@interfaces/service/ISetupResult';

export class NotificationService extends BaseService {
	public constructor() {
		super('NotificationService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					notifications: state.notifications,
				};
			},
		});
	}

	public setup(): Observable<ISetupResult> {
		super.setup();

		return getNotifications().pipe(
			tap((notifications) => {
				if (notifications.isSuccess) {
					this.setStoreProperty('notifications', notifications.value);
				}
			}),
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	// region Notifications

	public setNotification(notification: NotificationDTO): void {
		this.updateStore('notifications', notification);
	}

	public getNotifications(): Observable<NotificationDTO[]> {
		return this.stateChanged.pipe(map((state: IStoreState) => state?.notifications ?? []));
	}

	public hideNotification(id: number): void {
		const notifications = this.getState().notifications;
		const notificationIndex = notifications.findIndex((x) => x.id === id);
		if (notificationIndex > -1) {
			notifications[notificationIndex].hidden = true;
			this.setState({ notifications }, 'Set Notification Hidden');
		}
		hideNotification(id).pipe(take(1)).subscribe();
	}

	public clearAllNotifications(): void {
		this.setState({ notifications: [] }, 'Clear All Notifications');
		clearAllNotifications().pipe(take(1)).subscribe();
	}

	// endregion
}

export default new NotificationService();
