import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { checkResponse, preApiRequest } from '@api/baseApi';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';

const logText = 'From PlexLibraryAPI => ';
const apiPath = '/plexLibrary';

export function getPlexLibrary(libraryId: number, plexAccountId: number): Observable<PlexLibraryDTO | null> {
	preApiRequest(logText, 'getPlexLibrary');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/${libraryId}?plexAccountId=${plexAccountId}`);
	return checkResponse<PlexLibraryDTO>(result, logText, 'getPlexLibrary');
}

export function getPlexLibraryInServer(libraryId: number, plexAccountId: number): Observable<PlexServerDTO | null> {
	preApiRequest(logText, 'getPlexLibraryInServer');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/inserver/${libraryId}?plexAccountId=${plexAccountId}`);
	return checkResponse<PlexServerDTO>(result, logText, 'getPlexLibraryInServer');
}

export function refreshPlexLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
	preApiRequest(logText, 'refreshPlexLibrary');
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/refresh/`, {
		plexLibraryId: libraryId,
	});
	return checkResponse<PlexLibraryDTO | null>(result, logText, 'refreshPlexLibrary');
}
