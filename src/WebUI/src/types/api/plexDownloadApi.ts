import Log from 'consola';
import IDownloadTask from '@dto/IDownloadTask';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';

const logText = 'From PlexDownloadApi => ';
const apiPath = '/download';

export function downloadPlexMovie(movieId: number, plexAccountId: number): Observable<boolean> {
	if (plexAccountId <= 0) {
		Log.error(`${logText}downloadPlexMovie: invalid plexAccountId of ${plexAccountId}`);
		return of(false);
	}

	if (movieId <= 0) {
		Log.error(`${logText}downloadPlexMovie: invalid libraryId of ${movieId}`);
		return of(false);
	}

	Log.debug(`${logText}downloadPlexMovie: Sending request with movieId ${movieId} and plexAccountId ${plexAccountId}`);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/movie`, {
		plexAccountId,
		plexMovieId: movieId,
	});

	return result.pipe(
		tap((res) => Log.debug(`${logText}downloadPlexMovie response:`, res.data)),
		map((res: AxiosResponse) => res.data),
	);
}

export function deleteDownloadTask(downloadTaskId: number): Observable<boolean> {
	if (downloadTaskId <= 0) {
		Log.error(`${logText}deleteDownloadTask: invalid downloadTaskId of ${downloadTaskId}`);
		return of(false);
	}

	Log.debug(`${logText}deleteDownloadTask: Sending delete request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.delete(`${apiPath}/${downloadTaskId}`);

	return result.pipe(
		tap((res) => Log.debug(`${logText}deleteDownloadTask response:`, res.data)),
		map((res: AxiosResponse) => res.data),
	);
}

export function getAllDownloads(): Observable<IDownloadTask[]> {
	Log.debug(`${logText}getAllDownloads: Sending request`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/`);

	return result.pipe(
		tap((res) => Log.debug(`${logText}getAllDownloads response:`, res.data)),
		map((res: AxiosResponse) => res.data),
	);
}
