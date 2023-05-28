import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { NOTIFICATION_RELATIVE_PATH } from '@api-urls';

const logText = 'From notificationApi => ';

export function getNotifications(): Observable<ResultDTO<NotificationDTO[]>> {
	return PlexRipperAxios.get<NotificationDTO[]>({
		url: `${NOTIFICATION_RELATIVE_PATH}`,
		apiCategory: logText,
		apiName: getNotifications.name,
	});
}

export function hideNotification(id: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.put<boolean>({
		url: `${NOTIFICATION_RELATIVE_PATH}/${id}`,
		apiCategory: logText,
		apiName: hideNotification.name,
	});
}

export function clearAllNotifications(): Observable<ResultDTO> {
	return PlexRipperAxios.post({
		url: `${NOTIFICATION_RELATIVE_PATH}/clear`,
		apiCategory: logText,
		apiName: clearAllNotifications.name,
	});
}
