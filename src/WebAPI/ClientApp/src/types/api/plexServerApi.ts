import { Observable } from 'rxjs';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From PlexServerAPI => ';

export function getPlexServer(serverId: number): Observable<ResultDTO<PlexServerDTO | null>> {
	return PlexRipperAxios.get<PlexServerDTO>({
		url: `${PLEX_SERVER_RELATIVE_PATH}/${serverId}`,
		apiCategory: logText,
		apiName: getPlexServer.name,
	});
}

export function getPlexServers(): Observable<ResultDTO<PlexServerDTO[]>> {
	return PlexRipperAxios.get<PlexServerDTO[]>({
		url: PLEX_SERVER_RELATIVE_PATH,
		apiCategory: logText,
		apiName: getPlexServers.name,
	});
}

export function checkPlexServer(serverId: number): Observable<ResultDTO<PlexServerStatusDTO | null>> {
	return PlexRipperAxios.get<PlexServerStatusDTO>({
		url: `${PLEX_SERVER_RELATIVE_PATH}/${serverId}/check`,
		apiCategory: logText,
		apiName: checkPlexServer.name,
	});
}

export function syncPlexServer(serverId: number, forceSync: boolean = false): Observable<ResultDTO> {
	return PlexRipperAxios.get<void>({
		url: `${PLEX_SERVER_RELATIVE_PATH}/${serverId}/sync?forceSync=${forceSync}`,
		apiCategory: logText,
		apiName: syncPlexServer.name,
	});
}
