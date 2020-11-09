import { ReplaySubject, Observable, merge } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import Log from 'consola';
import { switchMap, tap } from 'rxjs/operators';
import globalService from '@service/globalService';
import { getNotifications, hideNotification } from '@api/notificationApi';
import signalrService from '@service/signalrService';

export class NotificationService {
	private _notifications: NotificationDTO[] = [];
	private _notificationsSubject: ReplaySubject<NotificationDTO[]> = new ReplaySubject<NotificationDTO[]>();

	public constructor() {
		globalService
			.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving all notifications')),
				switchMap(() => getNotifications()),
			)
			.subscribe((value) => {
				this._notifications = value ?? [];
				this._notificationsSubject.next(this._notifications);
			});

		signalrService.getNotificationUpdates().subscribe((data) => {
			this._notifications.push(data);
			this._notificationsSubject.next(this._notifications);
		});
	}

	public getNotifications(): Observable<NotificationDTO[]> {
		return this._notificationsSubject.asObservable();
	}

	public hideNotification(id: number): void {
		hideNotification(id).subscribe();
		const notificationIndex = this._notifications.findIndex((x) => x.id === id);
		if (notificationIndex > -1) {
			this._notifications[notificationIndex].hidden = true;
		}
		this._notificationsSubject.next(this._notifications);
	}
}

const notificationService = new NotificationService();
export default notificationService;
