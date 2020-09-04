import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { DownloadTvShowDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From PlexDownloadApi => ';
const apiPath = '/download';

export function downloadPlexMovie(movieId: number, plexAccountId: number): Observable<boolean> {
	preApiRequest(logText, 'downloadPlexMovie', `Sending request with movieId ${movieId} and plexAccountId ${plexAccountId}`);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/movie`, {
		plexAccountId,
		plexMovieId: movieId,
	});
	return checkResponse<boolean>(result, logText, 'downloadPlexMovie');
}

export function downloadTvShow(mediaId: number, plexAccountId: number, type: PlexMediaType): Observable<boolean> {
	preApiRequest(logText, 'downloadTvShow', `Sending request with tvShowId ${mediaId} and plexAccountId ${plexAccountId}`);
	const command: DownloadTvShowDTO = { plexAccountId, plexMediaId: mediaId, type };
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/tvShow`, command);
	return checkResponse<boolean>(result, logText, 'downloadTvShow');
}

export function deleteDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'deleteDownloadTask', `Sending delete request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.delete(`${apiPath}/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'deleteDownloadTask');
}

export function stopDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'stopDownloadTask', `Sending stop request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/stop/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'stopDownloadTask');
}

export function startDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'startDownloadTask', `Sending restart request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/start/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'startDownloadTask');
}

export function restartDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'restartDownloadTask', `Sending restart request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/restart/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'restartDownloadTask');
}

export function clearDownloadTasks(): Observable<boolean> {
	preApiRequest(logText, 'clearDownloadTasks');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/clearcomplete`);
	return checkResponse<boolean>(result, logText, 'clearDownloadTasks');
}

export function getAllDownloads(): Observable<PlexServerDTO[]> {
	preApiRequest(logText, 'getAllDownloads');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}`);
	return checkResponse<PlexServerDTO[]>(result, logText, 'getAllDownloads');
}
