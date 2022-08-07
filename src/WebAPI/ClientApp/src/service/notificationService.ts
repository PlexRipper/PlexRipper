import Log from 'consola';
import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { BaseService, GlobalService } from '@service';
import { clearAllNotifications, getNotifications, hideNotification } from '@api/notificationApi';
import IStoreState from '@interfaces/service/IStoreState';
import { Context } from '@nuxt/types';

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

	public setup(nuxtContext: Context, callBack: (name: string) => void): void {
		super.setNuxtContext(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving all notifications')),
				switchMap(() => getNotifications()),
			)
			.subscribe((result) => {
				if (result.isSuccess) {
					this.setState({ notifications: result.value }, 'Set Notifications');
				}
				callBack(this._name);
			});
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

const notificationService = new NotificationService();
export default notificationService;
