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
	preApiRequest(logText, 'downloadMedia');
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/download`, downloadMediaCommand);
	return checkResponse<boolean>(result, logText, 'downloadMedia');
}

// region Commands
export function deleteDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'deleteDownloadTasks');
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/delete`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'deleteDownloadTask');
}

export function clearDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'clearDownloadTasks');
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/clear`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'clearDownloadTasks');
}

export function stopDownloadTasks(downloadTaskIds: number[]): Observable<boolean> {
	preApiRequest(logText, 'stopDownloadTasks');
	const result: Observable<AxiosResponse> = Axios.post(`${apiPath}/stop`, downloadTaskIds);
	return checkResponse<boolean>(result, logText, 'stopDownloadTasks');
}
// endregion

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
