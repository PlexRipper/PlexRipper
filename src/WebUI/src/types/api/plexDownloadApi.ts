import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { DownloadTaskDTO } from '@dto/mainApi';
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

export function deleteDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'deleteDownloadTask', `Sending delete request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.delete(`${apiPath}/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'deleteDownloadTask');
}

export function getAllDownloads(): Observable<DownloadTaskDTO[]> {
	preApiRequest(logText, 'getAllDownloads');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/`);
	return checkResponse<DownloadTaskDTO[]>(result, logText, 'getAllDownloads');
}
