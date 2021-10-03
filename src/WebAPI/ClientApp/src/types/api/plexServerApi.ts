import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import { checkResponse, preApiRequest } from '@api/baseApi';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';

const logText = 'From PlexServerAPI => ';
const apiPath = '/PlexServer';

export function getPlexServer(serverId: number): Observable<ResultDTO<PlexServerDTO | null>> {
	preApiRequest(logText, 'getPlexServer');
	const result = Axios.get(`${apiPath}/${serverId}`);
	return checkResponse<ResultDTO<PlexServerDTO>>(result, logText, 'getPlexServer');
}

export function syncPlexServer(serverId: number, forceSync: boolean = false): Observable<ResultDTO> {
	preApiRequest(logText, 'syncPlexServer');
	const result = Axios.get(`${apiPath}/${serverId}/sync?forceSync=${forceSync}`);
	return checkResponse<ResultDTO>(result, logText, 'syncPlexServer');
}

export function getPlexServers(): Observable<ResultDTO<PlexServerDTO[]>> {
	preApiRequest(logText, 'getPlexServers');
	const result = Axios.get(`${apiPath}`);
	return checkResponse<ResultDTO<PlexServerDTO[]>>(result, logText, 'getPlexServers');
}
export function checkPlexServer(serverId: number): Observable<ResultDTO<PlexServerStatusDTO | null>> {
	preApiRequest(logText, 'checkPlexServer');
	const result = Axios.get(`${apiPath}/${serverId}/check`);
	return checkResponse<ResultDTO<PlexServerStatusDTO>>(result, logText, 'checkPlexServer');
}
