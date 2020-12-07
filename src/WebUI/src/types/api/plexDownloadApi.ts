import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { DownloadMediaDTO, DownloadTaskDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From PlexDownloadApi => ';
const apiPath = '/download';

export function getDownloadTasksInServer(): Observable<PlexServerDTO[]> {
	preApiRequest(logText, 'getDownloadTasksInServer');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/inserver`);
	return checkResponse<PlexServerDTO[]>(result, logText, 'getDownloadTasksInServer');
}

export function getAllDownloads(): Observable<DownloadTaskDTO[]> {
	preApiRequest(logText, 'getAllDownloads');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}`);
	return checkResponse<DownloadTaskDTO[]>(result, logText, 'getAllDownloads');
}

export function downloadMedia(mediaId: number, plexAccountId: number, type: PlexMediaType): Observable<boolean> {
	preApiRequest(logText, 'downloadMedia', `Sending request with tvShowId ${mediaId} and plexAccountId ${plexAccountId}`);
	const command: DownloadMediaDTO = { plexAccountId, plexMediaId: mediaId, type };
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/download`, command);
	return checkResponse<boolean>(result, logText, 'downloadMedia');
}

export function deleteDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'deleteDownloadTask', `Sending delete request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.delete(`${apiPath}/delete/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'deleteDownloadTask');
}

export function deleteDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'deleteDownloadTask', `Sending bulk delete request with ${downloadTaskIds.length} downloadTaskIds`);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/delete/`, downloadTaskIds);
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

export function pauseDownloadTask(downloadTaskId: number): Observable<boolean> {
	preApiRequest(logText, 'pauseDownloadTask', `Sending restart request with downloadTaskId ${downloadTaskId}`);
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/pause/${downloadTaskId}`);
	return checkResponse<boolean>(result, logText, 'pauseDownloadTask');
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
