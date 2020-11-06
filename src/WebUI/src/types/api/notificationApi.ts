import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { NotificationDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';

const logText = 'From notificationApi => ';
const apiPath = '/notification';

export function getNotifications(): Observable<NotificationDTO[]> {
	preApiRequest(logText, 'getNotifications');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}`);
	return checkResponse<NotificationDTO[]>(result, logText, 'getNotifications');
}

export function hideNotification(id: number): Observable<boolean> {
	preApiRequest(logText, 'hideNotification' + id);
	const result: Observable<AxiosResponse> = Axios.put(`${apiPath}/${id}`);
	return checkResponse<boolean>(result, logText, 'hideNotification');
}
