import { Observable } from 'rxjs';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_SERVER_CONNECTION_RELATIVE_PATH } from '@api-urls';
import { PlexServerConnectionDTO, PlexServerStatusDTO } from '@dto/mainApi';

export function getPlexServerConnection(serverConnectionId: number): Observable<ResultDTO<PlexServerConnectionDTO | null>> {
	return PlexRipperAxios.get<PlexServerConnectionDTO>({
		url: `${PLEX_SERVER_CONNECTION_RELATIVE_PATH}/${serverConnectionId}`,
		apiName: getPlexServerConnection.name,
	});
}

export function getPlexServerConnections(): Observable<ResultDTO<PlexServerConnectionDTO[]>> {
	return PlexRipperAxios.get<PlexServerConnectionDTO[]>({
		url: PLEX_SERVER_CONNECTION_RELATIVE_PATH,
		apiName: getPlexServerConnections.name,
	});
}

export function checkPlexServerConnection(serverConnectionId: number): Observable<ResultDTO<PlexServerStatusDTO>> {
	return PlexRipperAxios.get<PlexServerStatusDTO>({
		url: `${PLEX_SERVER_CONNECTION_RELATIVE_PATH}/check/${serverConnectionId}`,
		apiName: checkPlexServerConnection.name,
	});
}
