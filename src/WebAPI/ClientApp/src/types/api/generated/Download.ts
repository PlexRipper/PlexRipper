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
	ResultDTO,
	ServerDownloadProgressDTO,
} from './data-contracts';
import { ContentType, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import queryString from 'query-string';
import { from } from 'rxjs';

export class Download {
	/**
 * No description
 *
 * @tags Download
 * @name ClearCompletedDownloadTasksEndpoint
 * @request POST:/api/Download/clear

 */
	clearCompletedDownloadTasksEndpoint = (data: string[], params: RequestParams = {}) =>
		from(
			Axios.request<number>({
				url: `/api/Download/clear`,
				method: 'POST',
				data: data,
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
			Axios.request<ResultDTO>({
				url: `/api/Download/download`,
				method: 'POST',
				data: data,
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
			Axios.request<ResultDTO>({
				url: `/api/Download/delete`,
				method: 'DELETE',
				data: data,
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
			Axios.request<DownloadTaskDTO>({
				url: `/api/Download/detail/${downloadTaskGuid}`,
				method: 'GET',
				params: query,
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
			Axios.request<ServerDownloadProgressDTO[]>({
				url: `/api/Download`,
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
			Axios.request<ResultDTO>({
				url: `/api/Download/pause/${downloadTaskGuid}`,
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
			Axios.request<DownloadPreviewDTO[]>({
				url: `/api/Download/preview`,
				method: 'POST',
				data: data,
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
			Axios.request<ResultDTO>({
				url: `/api/Download/restart/${downloadTaskGuid}`,
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
			Axios.request<ResultDTO>({
				url: `/api/Download/start/${downloadTaskGuid}`,
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
			Axios.request<ResultDTO>({
				url: `/api/Download/stop/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}

export class DownloadPaths {
	static clearCompletedDownloadTasksEndpoint = () => queryString.stringifyUrl({ url: `/api/Download/clear` });

	static createDownloadTasksEndpoint = () => queryString.stringifyUrl({ url: `/api/Download/download` });

	static deleteDownloadTaskEndpoint = () => queryString.stringifyUrl({ url: `/api/Download/delete` });

	static getDownloadTaskByGuidEndpoint = (
		downloadTaskGuid: string,
		query?: {
			/** @default 0 */
			type?: DownloadTaskType;
		},
	) => queryString.stringifyUrl({ url: `/api/Download/detail/${downloadTaskGuid}`, query });

	static getAllDownloadTasksEndpoint = () => queryString.stringifyUrl({ url: `/api/Download` });

	static pauseDownloadTaskEndpoint = (downloadTaskGuid: string) =>
		queryString.stringifyUrl({ url: `/api/Download/pause/${downloadTaskGuid}` });

	static getDownloadPreviewEndpoint = () => queryString.stringifyUrl({ url: `/api/Download/preview` });

	static restartDownloadTaskEndpoint = (downloadTaskGuid: string) =>
		queryString.stringifyUrl({ url: `/api/Download/restart/${downloadTaskGuid}` });

	static startDownloadTaskEndpoint = (downloadTaskGuid: string) =>
		queryString.stringifyUrl({ url: `/api/Download/start/${downloadTaskGuid}` });

	static stopDownloadTaskEndpoint = (downloadTaskGuid: string) =>
		queryString.stringifyUrl({ url: `/api/Download/stop/${downloadTaskGuid}` });
}
