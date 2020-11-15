import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { checkResponse, preApiRequest } from '@api/baseApi';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';

const logText = 'From PlexServerAPI => ';
const apiPath = '/PlexServer';

export function getPlexServer(serverId: number): Observable<PlexServerDTO | null> {
	preApiRequest(logText, 'getPlexServer');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/${serverId}`);
	return checkResponse<PlexServerDTO>(result, logText, 'getPlexServer');
}
export function checkPlexServer(serverId: number): Observable<PlexServerStatusDTO | null> {
	preApiRequest(logText, 'checkPlexServer');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/${serverId}/check`);
	return checkResponse<PlexServerStatusDTO>(result, logText, 'checkPlexServer');
}
