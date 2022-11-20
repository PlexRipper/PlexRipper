import { Observable } from 'rxjs';
import { DOWNLOAD_RELATIVE_PATH } from '@api-urls';
import { DownloadMediaDTO, DownloadTaskDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From PlexDownloadApi => ';

export function getAllDownloads(): Observable<ResultDTO<ServerDownloadProgressDTO[]>> {
	return PlexRipperAxios.get<ServerDownloadProgressDTO[]>({
		url: DOWNLOAD_RELATIVE_PATH,
		apiCategory: logText,
		apiName: getAllDownloads.name,
	});
}

export function downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.post<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/download`,
		apiCategory: logText,
		apiName: downloadMedia.name,
		data: downloadMediaCommand,
	});
}

// region Commands
export function restartDownloadTasks(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/restart/${downloadTaskId}`,
		apiCategory: logText,
		apiName: restartDownloadTasks.name,
	});
}

export function deleteDownloadTasks(downloadTaskIds: number[]): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.post<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/delete`,
		apiCategory: logText,
		apiName: deleteDownloadTasks.name,
		data: downloadTaskIds,
	});
}

export function clearDownloadTasks(downloadTaskIds: number[]): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.post<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/clear`,
		apiCategory: logText,
		apiName: clearDownloadTasks.name,
		data: downloadTaskIds,
	});
}

export function stopDownloadTasks(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/stop/${downloadTaskId}`,
		apiCategory: logText,
		apiName: stopDownloadTasks.name,
	});
}

export function startDownloadTask(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/start/${downloadTaskId}`,
		apiCategory: logText,
		apiName: startDownloadTask.name,
	});
}

export function pauseDownloadTask(downloadTaskId: number): Observable<ResultDTO<boolean>> {
	return PlexRipperAxios.get<boolean>({
		url: `${DOWNLOAD_RELATIVE_PATH}/pause/${downloadTaskId}`,
		apiCategory: logText,
		apiName: pauseDownloadTask.name,
	});
}

export function detailDownloadTask(downloadTaskId: number): Observable<ResultDTO<DownloadTaskDTO>> {
	return PlexRipperAxios.get<DownloadTaskDTO>({
		url: `${DOWNLOAD_RELATIVE_PATH}/detail/${downloadTaskId}`,
		apiCategory: logText,
		apiName: detailDownloadTask.name,
	});
}

// endregion
