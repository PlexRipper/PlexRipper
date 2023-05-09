import { Observable } from 'rxjs';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
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

export function getPlexLibrary(
	libraryId: number,
	plexAccountId: number,
	allMedia = false,
): Observable<ResultDTO<PlexLibraryDTO>> {
	return PlexRipperAxios.get<PlexLibraryDTO>({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/${libraryId}?plexAccountId=${plexAccountId}&allMedia=${allMedia}`,
		apiCategory: logText,
		apiName: getPlexLibrary.name,
	});
}

export function refreshPlexLibrary(libraryId: number): Observable<ResultDTO<PlexLibraryDTO | null>> {
	return PlexRipperAxios.post({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/refresh/`,
		apiCategory: logText,
		apiName: refreshPlexLibrary.name,
		data: {
			plexLibraryId: libraryId,
		},
	});
}

export function updateDefaultDestination(libraryId: number, folderPathId: number): Observable<ResultDTO> {
	return PlexRipperAxios.put({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/settings/default/destination`,
		apiCategory: logText,
		apiName: updateDefaultDestination.name,
		data: {
			libraryId,
			folderPathId,
		},
	});
}
