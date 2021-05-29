import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { DownloadMediaDTO, DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';
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

export function downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): Observable<boolean> {
	preApiRequest(logText, 'downloadMedia', downloadMediaCommand);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/download`, downloadMediaCommand);
	return checkResponse<boolean>(result, logText, 'downloadMedia');
}

// region Commands
export function restartDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'restartDownloadTasks', downloadTaskIds);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/restart`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'restartDownloadTasks');
}

export function deleteDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'deleteDownloadTasks', downloadTaskIds);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/delete`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'deleteDownloadTask');
}

export function clearDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'clearDownloadTasks', downloadTaskIds);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/clear`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'clearDownloadTasks');
}

export function stopDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'stopDownloadTasks', downloadTaskIds);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/stop`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'stopDownloadTasks');
}

export function startDownloadTask(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'startDownloadTask', downloadTaskIds);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/start/`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'startDownloadTask');
}

export function pauseDownloadTask(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'pauseDownloadTask', downloadTaskIds);
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/pause/`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'pauseDownloadTask');
}
// endregion
