import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import { checkResponse, preApiRequest } from '@api/baseApi';
import { InspectServerDTO, PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';

const logText = 'From PlexServerAPI => ';
const apiPath = '/PlexServer';

export function getPlexServer(serverId: number): Observable<ResultDTO<PlexServerDTO | null>> {
	preApiRequest(logText, 'getPlexServer');
	const result = Axios.get(`${apiPath}/${serverId}`);
	return checkResponse<ResultDTO<PlexServerDTO>>(result, logText, 'getPlexServer');
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

export function inspectPlexServers(plexAccountId: number, plexServerIds: number[]): Observable<ResultDTO> {
	preApiRequest(logText, 'checkPlexServer');
	const result = Axios.post(`${apiPath}/inspect`, {
		plexAccountId,
		plexServerIds,
	} as InspectServerDTO);
	return checkResponse<ResultDTO>(result, logText, 'checkPlexServer');
}
