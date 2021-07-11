import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import Log from 'consola';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { BaseService, GlobalService, SignalrService } from '@service';
import { getNotifications, hideNotification } from '@api/notificationApi';
import IStoreState from '@interfaces/IStoreState';
import { Context } from '@nuxt/types';

export class NotificationService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					notifications: state.notifications,
					alerts: state.alerts,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving all notifications')),
				switchMap(() => getNotifications()),
			)
			.subscribe((value) => {
				this.setState({ notifications: value }, 'Set Notifications');
			});

		SignalrService.getNotificationUpdates().subscribe((data) => {
			this.setState({ notifications: { ...this.getState().notifications, ...data } }, 'Add Notifications');
		});
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

	public showAlert(): void {
		Log.error('Alert Shown');
	}
}

const notificationService = new NotificationService();
export default notificationService;
