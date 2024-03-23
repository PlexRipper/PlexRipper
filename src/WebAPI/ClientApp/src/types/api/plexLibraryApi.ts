import { Observable } from 'rxjs';
import type { PlexLibraryDTO } from '@dto/mainApi';
import type ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_LIBRARY_RELATIVE_PATH } from '@api-urls';

const logText = 'From PlexLibraryAPI => ';

export function getAllPlexLibraries(): Observable<ResultDTO<PlexLibraryDTO[]>> {
	return PlexRipperAxios.get<PlexLibraryDTO[]>({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}`,
		apiCategory: logText,
		apiName: getAllPlexLibraries.name,
	});
}

export function getPlexLibrary(libraryId: number): Observable<ResultDTO<PlexLibraryDTO>> {
	return PlexRipperAxios.get<PlexLibraryDTO>({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/${libraryId}`,
		apiCategory: logText,
		apiName: getPlexLibrary.name,
	});
}

export function reSyncPlexLibrary(libraryId: number): Observable<ResultDTO<PlexLibraryDTO | null>> {
	return PlexRipperAxios.get({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/refresh/${libraryId}`,
		apiCategory: logText,
		apiName: reSyncPlexLibrary.name,
	});
}

export function updateDefaultDestination(libraryId: number, folderPathId: number): Observable<ResultDTO> {
	return PlexRipperAxios.put({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/default/destination/{folderPathId}`,
		apiCategory: logText,
		apiName: updateDefaultDestination.name,
		data: {
			libraryId,
			folderPathId,
		},
	});
}
