import { Observable } from 'rxjs';
import { DownloadMediaDTO, DownloadTaskDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From PlexDownloadApi => ';
const apiPath = '/download';

export function getAllDownloads(): Observable<ResultDTO<ServerDownloadProgressDTO[]>> {
	return PlexRipperAxios.get<ServerDownloadProgressDTO[]>({
		url: `${apiPath}`,
		apiCategory: logText,
		apiName: getAllDownloads.name,
	});
}

export function downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.post<boolean>({
		url: `${apiPath}/download`,
		apiCategory: logText,
		apiName: downloadMedia.name,
		data: downloadMediaCommand,
	});
}

// region Commands
export function restartDownloadTasks(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${apiPath}/restart/${downloadTaskId}`,
		apiCategory: logText,
		apiName: restartDownloadTasks.name,
	});
}

export function deleteDownloadTasks(downloadTaskIds: number[]): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.post<boolean>({
		url: `${apiPath}/delete`,
		apiCategory: logText,
		apiName: deleteDownloadTasks.name,
		data: downloadTaskIds,
	});
}

export function clearDownloadTasks(downloadTaskIds: number[]): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.post<boolean>({
		url: `${apiPath}/clear`,
		apiCategory: logText,
		apiName: clearDownloadTasks.name,
		data: downloadTaskIds,
	});
}

export function stopDownloadTasks(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${apiPath}/stop/${downloadTaskId}`,
		apiCategory: logText,
		apiName: stopDownloadTasks.name,
	});
}

export function startDownloadTask(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${apiPath}/start/${downloadTaskId}`,
		apiCategory: logText,
		apiName: startDownloadTask.name,
	});
}

export function pauseDownloadTask(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${apiPath}/pause/${downloadTaskId}`,
		apiCategory: logText,
		apiName: pauseDownloadTask.name,
	});
}

export function detailDownloadTask(downloadTaskId: number): Observable<ResultDTO<DownloadTaskDTO>> {
	return PlexRipperAxios.get<DownloadTaskDTO>({
		url: `${apiPath}/detail/${downloadTaskId}`,
		apiCategory: logText,
		apiName: detailDownloadTask.name,
	});
}

// endregion
