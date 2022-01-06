import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { DownloadMediaDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From PlexDownloadApi => ';
const apiPath = '/download';

export function getAllDownloads(): Observable<ResultDTO<ServerDownloadProgressDTO[]>> {
	preApiRequest(logText, 'getAllDownloads');
	const result = Axios.get(`${apiPath}`);
	return checkResponse<ResultDTO<ServerDownloadProgressDTO[]>>(result, logText, 'getAllDownloads');
}

export function downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'downloadMedia', downloadMediaCommand);
	const result = Axios.post(`${apiPath}/download`, downloadMediaCommand);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'downloadMedia');
}

// region Commands
export function restartDownloadTasks(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'restartDownloadTasks', downloadTaskId);
	const result = Axios.get(`${apiPath}/restart/${downloadTaskId}`);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'restartDownloadTasks');
}

export function deleteDownloadTasks(downloadTaskIds: number[]): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'deleteDownloadTasks', downloadTaskIds);
	const result = Axios.post(`${apiPath}/delete`, downloadTaskIds);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'deleteDownloadTask');
}

export function clearDownloadTasks(downloadTaskIds: number[]): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'clearDownloadTasks', downloadTaskIds);
	const result = Axios.post(`${apiPath}/clear`, downloadTaskIds);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'clearDownloadTasks');
}

export function stopDownloadTasks(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'stopDownloadTasks', downloadTaskId);
	const result = Axios.get(`${apiPath}/stop/${downloadTaskId}`);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'stopDownloadTasks');
}

export function startDownloadTask(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'startDownloadTask', downloadTaskId);
	const result = Axios.get(`${apiPath}/start/${downloadTaskId}`);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'startDownloadTask');
}

export function pauseDownloadTask(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'pauseDownloadTask', downloadTaskId);
	const result = Axios.get(`${apiPath}/pause/${downloadTaskId}`);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'pauseDownloadTask');
}
// endregion
