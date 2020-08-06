import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { PlexLibraryDTO } from '@dto/api/mainApi';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From PlexLibraryAPI => ';
const apiPath = '/plexLibrary';

export function getPlexLibrary(libraryId: number, plexAccountId: number): Observable<PlexLibraryDTO | null> {
	preApiRequest(logText, 'getPlexLibrary');
	const result: Observable<AxiosResponse> = Axios.get<PlexLibraryDTO>(`${apiPath}/${libraryId}?plexAccountId=${plexAccountId}`);
	return checkResponse<PlexLibraryDTO>(result, logText, 'getPlexLibrary');
}

export function refreshPlexLibrary(libraryId: number, plexAccountId: number): Observable<PlexLibraryDTO | null> {
	preApiRequest(logText, 'getPlexLibrary');
	const result: Observable<AxiosResponse> = Axios.post<PlexLibraryDTO>(`${apiPath}/refresh/`, {
		plexAccountId,
		plexLibraryId: libraryId,
	});
	return checkResponse<PlexLibraryDTO>(result, logText, 'getPlexLibrary');
}
