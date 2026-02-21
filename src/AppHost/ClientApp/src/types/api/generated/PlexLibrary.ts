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

import type {
  BaseResultDTO,
  LibrarySyncJobQueueDTO,
  PlexLibraryDTO,
  PlexMediaMetadataDTO,
  PlexMediaStatisticsDTO,
  PlexMediaType,
  VideoQuality,
} from "./data-contracts";

import { apiCheckPipe, axiosObservable } from "@api/base";
import queryString from "query-string";

export class PlexLibrary {
  /**
   * No description
   * * @tags Plexlibrary
   * @name GetPlexLibraryByIdEndpoint
   * @request GET:/api/PlexLibrary/{PlexLibraryId}
   * @secure
   */
  getPlexLibraryByIdEndpoint = (
    plexLibraryId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexLibraryDTO>({
      url: `/api/PlexLibrary/${plexLibraryId}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexLibraryDTO>);

  /**
   * No description
   * * @tags Plexlibrary
   * @name GetAllPlexLibrariesEndpoint
   * @request GET:/api/PlexLibrary
   * @secure
   */
  getAllPlexLibrariesEndpoint = (params: RequestParams = {}) =>
    axiosObservable<PlexLibraryDTO[]>({
      url: `/api/PlexLibrary`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexLibraryDTO[]>);

  /**
   * No description
   * * @tags Plexlibrary
   * @name GetPlexLibraryMediaEndpoint
   * @request GET:/api/PlexLibrary/{PlexLibraryId}/media
   * @secure
   */
  getPlexLibraryMediaEndpoint = (
    plexLibraryId: number,
    query: {
      /**
       * @format int32
       * @default 0
       */
      countryId: number;
      /** @default false */
      filterOfflineMedia: boolean;
      /** @default false */
      filterOwnedMedia: boolean;
      /**
       * @format int32
       * @default 0
       */
      genreId: number;
      /**
       * @format int32
       * @default 0
       */
      page: number;
      /** @default -1 */
      quality: VideoQuality;
      /**
       * @format int32
       * @default 0
       */
      roleId: number;
      /**
       * @format int32
       * @default 0
       */
      size: number;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexMediaStatisticsDTO>({
      url: `/api/PlexLibrary/${plexLibraryId}/media`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexMediaStatisticsDTO>);

  /**
   * No description
   * * @tags Plexlibrary
   * @name GetLibraryMediaMetadata
   * @request GET:/api/PlexLibrary/{PlexLibraryId}/metadata
   * @secure
   */
  getLibraryMediaMetadata = (
    plexLibraryId: number,
    query: {
      /** @default 0 */
      mediaType: PlexMediaType;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexMediaMetadataDTO>({
      url: `/api/PlexLibrary/${plexLibraryId}/metadata`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexMediaMetadataDTO>);

  /**
   * No description
   * * @tags Plexlibrary
   * @name GetLibrarySyncStatusEndpoint
   * @request GET:/api/PlexLibrary/sync-status
   * @secure
   */
  getLibrarySyncStatusEndpoint = (params: RequestParams = {}) =>
    axiosObservable<LibrarySyncJobQueueDTO[]>({
      url: `/api/PlexLibrary/sync-status`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<LibrarySyncJobQueueDTO[]>);

  /**
   * No description
   * * @tags Plexlibrary
   * @name RefreshLibraryMediaEndpoint
   * @request GET:/api/PlexLibrary/refresh/{PlexLibraryId}
   * @secure
   */
  refreshLibraryMediaEndpoint = (
    plexLibraryId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexLibraryDTO>({
      url: `/api/PlexLibrary/refresh/${plexLibraryId}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexLibraryDTO>);

  /**
   * No description
   * * @tags Plexlibrary
   * @name SetPlexLibraryDefaultDestinationByIdEndpoint
   * @request GET:/api/PlexLibrary/{PlexLibraryId}/default/destination/{FolderPathId}
   * @secure
   */
  setPlexLibraryDefaultDestinationByIdEndpoint = (
    plexLibraryId: number,
    folderPathId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexLibrary/${plexLibraryId}/default/destination/${folderPathId}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);
}

export class PlexLibraryPaths {
  static getPlexLibraryByIdEndpoint = (plexLibraryId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexLibrary/${plexLibraryId}` });

  static getAllPlexLibrariesEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/PlexLibrary` });

  static getPlexLibraryMediaEndpoint = (
    plexLibraryId: number,
    query: {
      /**
       * @format int32
       * @default 0
       */
      countryId: number;
      /** @default false */
      filterOfflineMedia: boolean;
      /** @default false */
      filterOwnedMedia: boolean;
      /**
       * @format int32
       * @default 0
       */
      genreId: number;
      /**
       * @format int32
       * @default 0
       */
      page: number;
      /** @default -1 */
      quality: VideoQuality;
      /**
       * @format int32
       * @default 0
       */
      roleId: number;
      /**
       * @format int32
       * @default 0
       */
      size: number;
    },
  ) =>
    queryString.stringifyUrl({
      url: `/api/PlexLibrary/${plexLibraryId}/media`,
      query,
    });

  static getLibraryMediaMetadata = (
    plexLibraryId: number,
    query: {
      /** @default 0 */
      mediaType: PlexMediaType;
    },
  ) =>
    queryString.stringifyUrl({
      url: `/api/PlexLibrary/${plexLibraryId}/metadata`,
      query,
    });

  static getLibrarySyncStatusEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/PlexLibrary/sync-status` });

  static refreshLibraryMediaEndpoint = (plexLibraryId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexLibrary/refresh/${plexLibraryId}`,
    });

  static setPlexLibraryDefaultDestinationByIdEndpoint = (
    plexLibraryId: number,
    folderPathId: number,
  ) =>
    queryString.stringifyUrl({
      url: `/api/PlexLibrary/${plexLibraryId}/default/destination/${folderPathId}`,
    });
}
