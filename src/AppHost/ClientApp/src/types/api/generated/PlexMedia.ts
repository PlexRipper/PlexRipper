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
  PlexMediaDTO,
  PlexMediaSlimDTO,
  PlexMediaStatisticsDTO,
  PlexMediaType,
} from "./data-contracts";

import { apiCheckPipe, axiosObservable } from "@api/base";
import queryString from "query-string";

export class PlexMedia {
  /**
   * No description
   * * @tags Plexmedia
   * @name GetAllMediaByTypeEndpoint
   * @request GET:/api/PlexMedia
   * @secure
   */
  getAllMediaByTypeEndpoint = (
    query: {
      distinct?: boolean | null;
      filter?: string | null;
      /** @default false */
      filterOfflineMedia: boolean;
      /** @default false */
      filterOwnedMedia: boolean;
      groupBy?: string | null;
      having?: string | null;
      includeCount?: boolean | null;
      includes?: string | null;
      /** @default false */
      mediaType: PlexMediaType;
      mode?: string | null;
      /** @format int32 */
      page?: number | null;
      /** @format int32 */
      pageSize?: number | null;
      /** @format int32 */
      plexLibraryId: number;
      query?: string | null;
      select?: string | null;
      sort?: string | null;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexMediaStatisticsDTO>({
      url: `/api/PlexMedia`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexMediaStatisticsDTO>);

  /**
   * No description
   * * @tags Plexmedia
   * @name GetMediaDetailByIdEndpoint
   * @request GET:/api/PlexMedia/detail/{PlexMediaId}
   * @secure
   */
  getMediaDetailByIdEndpoint = (
    plexMediaId: number,
    query: {
      type: PlexMediaType;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexMediaDTO>({
      url: `/api/PlexMedia/detail/${plexMediaId}`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexMediaDTO>);

  /**
   * @description Proxies image bytes from Plex servers with CORS headers.
   * * @tags Plexmedia
   * @name GetPlexMediaThumbnailImageEndpoint
   * @summary Proxy Plex image
   * @request GET:/api/PlexMedia/thumbnail
   * @secure
   */
  getPlexMediaThumbnailImageEndpoint = (
    query: {
      /**
       * @format int32
       * @example 400
       */
      height: number;
      /**
       * @format int32
       * @example 57920
       */
      metaDataKey: number;
      /**
       * @format int32
       * @example 1756014789
       */
      plexKey: number;
      /**
       * @format int32
       * @example 1
       */
      plexServerId: number;
      /**
       * @format int32
       * @example 200
       */
      width: number;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<Blob>({
      url: `/api/PlexMedia/thumbnail`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "blob",
      ...params,
    }).pipe(apiCheckPipe<Blob>);

  /**
   * No description
   * * @tags Plexmedia
   * @name SearchPlexMediaEndpoint
   * @request GET:/api/PlexMedia/search
   * @secure
   */
  searchPlexMediaEndpoint = (
    query: {
      query: string;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexMediaSlimDTO[]>({
      url: `/api/PlexMedia/search`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexMediaSlimDTO[]>);
}

export class PlexMediaPaths {
  static getAllMediaByTypeEndpoint = (query: {
    distinct?: boolean | null;
    filter?: string | null;
    /** @default false */
    filterOfflineMedia: boolean;
    /** @default false */
    filterOwnedMedia: boolean;
    groupBy?: string | null;
    having?: string | null;
    includeCount?: boolean | null;
    includes?: string | null;
    /** @default false */
    mediaType: PlexMediaType;
    mode?: string | null;
    /** @format int32 */
    page?: number | null;
    /** @format int32 */
    pageSize?: number | null;
    /** @format int32 */
    plexLibraryId: number;
    query?: string | null;
    select?: string | null;
    sort?: string | null;
  }) => queryString.stringifyUrl({ url: `/api/PlexMedia`, query });

  static getMediaDetailByIdEndpoint = (
    plexMediaId: number,
    query: {
      type: PlexMediaType;
    },
  ) =>
    queryString.stringifyUrl({
      url: `/api/PlexMedia/detail/${plexMediaId}`,
      query,
    });

  static getPlexMediaThumbnailImageEndpoint = (query: {
    /**
     * @format int32
     * @example 400
     */
    height: number;
    /**
     * @format int32
     * @example 57920
     */
    metaDataKey: number;
    /**
     * @format int32
     * @example 1756014789
     */
    plexKey: number;
    /**
     * @format int32
     * @example 1
     */
    plexServerId: number;
    /**
     * @format int32
     * @example 200
     */
    width: number;
  }) => queryString.stringifyUrl({ url: `/api/PlexMedia/thumbnail`, query });

  static searchPlexMediaEndpoint = (query: { query: string }) =>
    queryString.stringifyUrl({ url: `/api/PlexMedia/search`, query });
}
