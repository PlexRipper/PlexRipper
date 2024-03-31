/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
	DownloadMediaDTO,
	DownloadPreviewDTO,
	DownloadTaskDTO,
	DownloadTaskType,
	ErrorResponse,
	ResultDTO,
	ServerDownloadProgressDTO,
} from './data-contracts';
import { ContentType, HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class Download<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Download
 * @name ClearCompletedDownloadTasksEndpoint
 * @request POST:/api/Download/clear

 */
	clearCompletedDownloadTasksEndpoint = (data: string[], params: RequestParams = {}) =>
		from(
			this.request<number, any>({
				path: `/api/Download/clear`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<number>);
	/**
 * No description
 *
 * @tags Download
 * @name CreateDownloadTasksEndpoint
 * @request POST:/api/Download/download

 */
	createDownloadTasksEndpoint = (data: DownloadMediaDTO[], params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/download`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Download
 * @name DeleteDownloadTaskEndpoint
 * @request DELETE:/api/Download/delete

 */
	deleteDownloadTaskEndpoint = (data: string[], params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/delete`,
				method: 'DELETE',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Download
 * @name GetDownloadTaskByGuidEndpoint
 * @request GET:/api/Download/detail/{downloadTaskGuid}

 */
	getDownloadTaskByGuidEndpoint = (
		downloadTaskGuid: string,
		query?: {
			/** @default 0 */
			type?: DownloadTaskType;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<DownloadTaskDTO, ResultDTO>({
				path: `/api/Download/detail/${downloadTaskGuid}`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<DownloadTaskDTO>);
	/**
 * No description
 *
 * @tags Download
 * @name GetAllDownloadTasksEndpoint
 * @request GET:/api/Download

 */
	getAllDownloadTasksEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<ServerDownloadProgressDTO[], ResultDTO>({
				path: `/api/Download`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ServerDownloadProgressDTO[]>);
	/**
 * No description
 *
 * @tags Download
 * @name PauseDownloadTaskEndpoint
 * @request GET:/api/Download/pause/{downloadTaskGuid}

 */
	pauseDownloadTaskEndpoint = (downloadTaskGuid: string, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/pause/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Download
 * @name GetDownloadPreviewEndpoint
 * @request POST:/api/Download/preview

 */
	getDownloadPreviewEndpoint = (data: DownloadMediaDTO[], params: RequestParams = {}) =>
		from(
			this.request<DownloadPreviewDTO[], ResultDTO>({
				path: `/api/Download/preview`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<DownloadPreviewDTO[]>);
	/**
 * No description
 *
 * @tags Download
 * @name RestartDownloadTaskEndpoint
 * @request GET:/api/Download/restart/{downloadTaskGuid}

 */
	restartDownloadTaskEndpoint = (downloadTaskGuid: string, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/restart/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Download
 * @name StartDownloadTaskEndpoint
 * @request GET:/api/Download/start/{downloadTaskGuid}

 */
	startDownloadTaskEndpoint = (downloadTaskGuid: string, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/Download/start/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Download
 * @name StopDownloadTaskEndpoint
 * @request GET:/api/Download/stop/{downloadTaskGuid}

 */
	stopDownloadTaskEndpoint = (downloadTaskGuid: string, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/stop/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
