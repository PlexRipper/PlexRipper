import { Observable } from 'rxjs';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import type { PlexServerDTO } from '@dto/mainApi';
import type ResultDTO from '@dto/ResultDTO';
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

export function syncPlexServer(serverId: number, forceSync = false): Observable<ResultDTO> {
	return PlexRipperAxios.get<void>({
		url: `${PLEX_SERVER_RELATIVE_PATH}/${serverId}/sync?forceSync=${forceSync}`,
		apiCategory: logText,
		apiName: syncPlexServer.name,
	});
}

export function inspectPlexServer(serverId: number): Observable<ResultDTO> {
	return PlexRipperAxios.get<void>({
		url: `${PLEX_SERVER_RELATIVE_PATH}/${serverId}/inspect`,
		apiCategory: logText,
		apiName: syncPlexServer.name,
	});
}

export function setPreferredPlexServerConnection(serverId: number, connectionId: number): Observable<ResultDTO> {
	return PlexRipperAxios.put<void>({
		url: `${PLEX_SERVER_RELATIVE_PATH}/${serverId}/preferred-connection/${connectionId}`,
		apiName: setPreferredPlexServerConnection.name,
	});
}
