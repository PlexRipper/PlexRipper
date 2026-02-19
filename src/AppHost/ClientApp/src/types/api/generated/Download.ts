/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import type { RequestParams } from "./http-client";
import { ContentType } from "./http-client";

import type {
  BaseResultDTO,
  CountResponseDTO,
  CreateDownloadTasksRequest,
  DownloadMediaDTO,
  DownloadPreviewContainerDTO,
  DownloadTaskDTO,
  DownloadTaskType,
  DownloadWorkerLogDTO,
  ServerDownloadProgressDTO,
} from "./data-contracts";

import { apiCheckPipe, axiosObservable } from "@api/base";
import queryString from "query-string";

export class Download {
  /**
   * No description
   * * @tags Download
   * @name ClearCompletedDownloadTasksEndpoint
   * @request POST:/api/Download/clear
   * @secure
   */
  clearCompletedDownloadTasksEndpoint = (
    data: string[],
    params: RequestParams = {},
  ) =>
    axiosObservable<CountResponseDTO>({
      url: `/api/Download/clear`,
      method: "POST",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<CountResponseDTO>);

  /**
   * No description
   * * @tags Download
   * @name CreateDownloadTasksEndpoint
   * @request POST:/api/Download/create
   * @secure
   */
  createDownloadTasksEndpoint = (
    data: CreateDownloadTasksRequest,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Download/create`,
      method: "POST",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Download
   * @name DeleteDownloadTaskEndpoint
   * @request DELETE:/api/Download/delete
   * @secure
   */
  deleteDownloadTaskEndpoint = (data: string[], params: RequestParams = {}) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Download/delete`,
      method: "DELETE",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Download
   * @name GetDownloadTaskByGuidEndpoint
   * @request GET:/api/Download/detail/{DownloadTaskGuid}
   * @secure
   */
  getDownloadTaskByGuidEndpoint = (
    downloadTaskGuid: string,
    query?: {
      /** @default 0 */
      type?: DownloadTaskType;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<DownloadTaskDTO>({
      url: `/api/Download/detail/${downloadTaskGuid}`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<DownloadTaskDTO>);

  /**
   * No description
   * * @tags Download
   * @name GetAllDownloadTasksEndpoint
   * @request GET:/api/Download
   * @secure
   */
  getAllDownloadTasksEndpoint = (params: RequestParams = {}) =>
    axiosObservable<ServerDownloadProgressDTO[]>({
      url: `/api/Download`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<ServerDownloadProgressDTO[]>);

  /**
   * No description
   * * @tags Download
   * @name GetDownloadTaskLogsByDownloadTaskIdEndpoint
   * @request GET:/api/Download/logs/{DownloadTaskGuid}
   * @secure
   */
  getDownloadTaskLogsByDownloadTaskIdEndpoint = (
    downloadTaskGuid: string,
    params: RequestParams = {},
  ) =>
    axiosObservable<DownloadWorkerLogDTO[]>({
      url: `/api/Download/logs/${downloadTaskGuid}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<DownloadWorkerLogDTO[]>);

  /**
   * No description
   * * @tags Download
   * @name PauseDownloadTaskEndpoint
   * @request GET:/api/Download/pause/{DownloadTaskGuid}
   * @secure
   */
  pauseDownloadTaskEndpoint = (
    downloadTaskGuid: string,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Download/pause/${downloadTaskGuid}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Download
   * @name GetDownloadPreviewEndpoint
   * @request POST:/api/Download/preview
   * @secure
   */
  getDownloadPreviewEndpoint = (
    data: DownloadMediaDTO[],
    params: RequestParams = {},
  ) =>
    axiosObservable<DownloadPreviewContainerDTO>({
      url: `/api/Download/preview`,
      method: "POST",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<DownloadPreviewContainerDTO>);

  /**
   * No description
   * * @tags Download
   * @name RestartDownloadTaskEndpoint
   * @request GET:/api/Download/restart/{DownloadTaskGuid}
   * @secure
   */
  restartDownloadTaskEndpoint = (
    downloadTaskGuid: string,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Download/restart/${downloadTaskGuid}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Download
   * @name StartDownloadTaskEndpoint
   * @request GET:/api/Download/start/{DownloadTaskGuid}
   * @secure
   */
  startDownloadTaskEndpoint = (
    downloadTaskGuid: string,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Download/start/${downloadTaskGuid}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Download
   * @name StopDownloadTaskEndpoint
   * @request GET:/api/Download/stop/{DownloadTaskGuid}
   * @secure
   */
  stopDownloadTaskEndpoint = (
    downloadTaskGuid: string,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Download/stop/${downloadTaskGuid}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);
}

export class DownloadPaths {
  static clearCompletedDownloadTasksEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Download/clear` });

  static createDownloadTasksEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Download/create` });

  static deleteDownloadTaskEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Download/delete` });

  static getDownloadTaskByGuidEndpoint = (
    downloadTaskGuid: string,
    query?: {
      /** @default 0 */
      type?: DownloadTaskType;
    },
  ) =>
    queryString.stringifyUrl({
      url: `/api/Download/detail/${downloadTaskGuid}`,
      query,
    });

  static getAllDownloadTasksEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Download` });

  static getDownloadTaskLogsByDownloadTaskIdEndpoint = (
    downloadTaskGuid: string,
  ) =>
    queryString.stringifyUrl({ url: `/api/Download/logs/${downloadTaskGuid}` });

  static pauseDownloadTaskEndpoint = (downloadTaskGuid: string) =>
    queryString.stringifyUrl({
      url: `/api/Download/pause/${downloadTaskGuid}`,
    });

  static getDownloadPreviewEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Download/preview` });

  static restartDownloadTaskEndpoint = (downloadTaskGuid: string) =>
    queryString.stringifyUrl({
      url: `/api/Download/restart/${downloadTaskGuid}`,
    });

  static startDownloadTaskEndpoint = (downloadTaskGuid: string) =>
    queryString.stringifyUrl({
      url: `/api/Download/start/${downloadTaskGuid}`,
    });

  static stopDownloadTaskEndpoint = (downloadTaskGuid: string) =>
    queryString.stringifyUrl({ url: `/api/Download/stop/${downloadTaskGuid}` });
}
