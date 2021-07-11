import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';
import ResultDTO from '@dto/ResultDTO';

const logText = 'From notificationApi => ';
const apiPath = '/notification';

export function getNotifications(): Observable<ResultDTO<NotificationDTO[]>> {
	preApiRequest(logText, 'getNotifications');
	const result = Axios.get(`${apiPath}`);
	return checkResponse<ResultDTO<NotificationDTO[]>>(result, logText, 'getNotifications');
}

export function hideNotification(id: number): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'hideNotification' + id);
	const result = Axios.put(`${apiPath}/${id}`);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'hideNotification');
}
