import { ReplaySubject, Observable, merge } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import Log from 'consola';
import { switchMap, tap } from 'rxjs/operators';
import globalService from '@service/globalService';
import { getNotifications, hideNotification } from '@api/notificationApi';
import signalrService from '@service/signalrService';

export class NotificationService {
	private _notifications: ReplaySubject<NotificationDTO[]> = new ReplaySubject<NotificationDTO[]>();

	public constructor() {
		globalService
			.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving all notifications')),
				switchMap(() => getNotifications()),
			)
			.subscribe((value) => {
				this._notifications.next(value ?? []);
			});
	}

	public getNotifications(): Observable<NotificationDTO[]> {
		return this._notifications.asObservable();
	}

	public hideNotification(id: number): void {
		hideNotification(id).subscribe();
	}
}

const notificationService = new NotificationService();
export default notificationService;
