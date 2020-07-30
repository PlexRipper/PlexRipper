import Log from 'consola';
import IPlexLibrary from '@dto/IPlexLibrary';
import { Observable, of } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map, tap } from 'rxjs/operators';

const logText = 'From PlexLibraryAPI => ';
const apiPath = '/plexLibrary';

export function getPlexLibrary(libraryId: number, plexAccountId: number): Observable<IPlexLibrary | null> {
	if (plexAccountId <= 0) {
		Log.error(`${logText}getPlexLibrary: invalid plexAccountId of ${plexAccountId}`);
		return of(null);
	}

	if (libraryId <= 0) {
		Log.error(`${logText}getPlexLibrary: invalid libraryId of ${libraryId}`);
		return of(null);
	}

	Log.debug(`${logText}getPlexLibrary: Sending request with libraryId ${libraryId} and plexAccountId ${plexAccountId}`);
	const result: Observable<AxiosResponse> = Axios.get<IPlexLibrary>(`${apiPath}/${libraryId}?plexAccountId=${plexAccountId}`);
	return result.pipe(
		tap(() => Log.debug(`${logText}getPlexLibrary`)),
		map((res: AxiosResponse) => res.data),
	);
}

export function refreshPlexLibrary(libraryId: number, plexAccountId: number): Observable<IPlexLibrary | null> {
	if (plexAccountId <= 0) {
		Log.error(`${logText}refreshPlexLibrary: invalid plexAccountId of ${plexAccountId}`);
		return of(null);
	}

	if (libraryId <= 0) {
		Log.error(`${logText}refreshPlexLibrary: invalid libraryId of ${libraryId}`);
		return of(null);
	}

	const result: Observable<AxiosResponse> = Axios.post<IPlexLibrary>(`${apiPath}/refresh/`, {
		plexAccountId,
		plexLibraryId: libraryId,
	});
	return result.pipe(
		tap(() => Log.debug(`${logText}refreshPlexLibrary`)),
		map((res: AxiosResponse) => res.data),
	);
}
