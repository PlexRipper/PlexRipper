import { Observable } from 'rxjs';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From PlexLibraryAPI => ';
const apiPath = '/plexLibrary';

export function getAllPlexLibraries(): Observable<ResultDTO<PlexLibraryDTO[]>> {
	return PlexRipperAxios.get<PlexLibraryDTO[]>({
		url: `${apiPath}`,
		apiCategory: logText,
		apiName: getAllPlexLibraries.name,
	});
}

export function getPlexLibrary(libraryId: number, plexAccountId: number): Observable<ResultDTO<PlexLibraryDTO>> {
	return PlexRipperAxios.get<PlexLibraryDTO>({
		url: `${apiPath}/${libraryId}?plexAccountId=${plexAccountId}`,
		apiCategory: logText,
		apiName: getPlexLibrary.name,
	});
}

export function getPlexLibraryInServer(libraryId: number, plexAccountId: number): Observable<ResultDTO<PlexServerDTO | null>> {
	return PlexRipperAxios.get<PlexServerDTO | null>({
		url: `${apiPath}/inserver/${libraryId}?plexAccountId=${plexAccountId}`,
		apiCategory: logText,
		apiName: getPlexLibraryInServer.name,
	});
}

export function refreshPlexLibrary(libraryId: number): Observable<ResultDTO<PlexLibraryDTO | null>> {
	return PlexRipperAxios.post({
		url: `${apiPath}/refresh/`,
		apiCategory: logText,
		apiName: refreshPlexLibrary.name,
		data: {
			plexLibraryId: libraryId,
		},
	});
}

export function updateDefaultDestination(libraryId: number, folderPathId: number): Observable<ResultDTO> {
	return PlexRipperAxios.put({
		url: `${apiPath}/settings/default/destination`,
		apiCategory: logText,
		apiName: updateDefaultDestination.name,
		data: {
			libraryId,
			folderPathId,
		},
	});
}
