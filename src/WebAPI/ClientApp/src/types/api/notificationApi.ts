import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From notificationApi => ';
const apiPath = '/notification';

export function getNotifications(): Observable<ResultDTO<NotificationDTO[]>> {
	return PlexRipperAxios.get<NotificationDTO[]>({
		url: `${apiPath}`,
		apiCategory: logText,
		apiName: getNotifications.name,
	});
}

export function hideNotification(id: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.put<boolean>({
		url: `${apiPath}/${id}`,
		apiCategory: logText,
		apiName: hideNotification.name,
	});
}

export function clearAllNotifications(): Observable<ResultDTO> {
	return PlexRipperAxios.post({
		url: `${apiPath}/clear`,
		apiCategory: logText,
		apiName: clearAllNotifications.name,
	});
}
