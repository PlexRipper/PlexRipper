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

import type { RequestParams } from "./http-client";

import type { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaStatisticsDTO, PlexMediaType } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

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
      filterOfflineMedia: boolean;
      filterOwnedMedia: boolean;
      mediaType: PlexMediaType;
      /**
       * @format int32
       * @default 0
       */
      page: number;
      /**
       * @format int32
       * @default 0
       */
      size: number;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<PlexMediaStatisticsDTO>({
        url: `/api/PlexMedia`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexMediaStatisticsDTO>);

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
    from(
      Axios.request<PlexMediaDTO>({
        url: `/api/PlexMedia/detail/${plexMediaId}`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexMediaDTO>);

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
    from(
      Axios.request<PlexMediaSlimDTO[]>({
        url: `/api/PlexMedia/search`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexMediaSlimDTO[]>);
}

export class PlexMediaPaths {
  static getAllMediaByTypeEndpoint = (query: {
    filterOfflineMedia: boolean;
    filterOwnedMedia: boolean;
    mediaType: PlexMediaType;
    /**
     * @format int32
     * @default 0
     */
    page: number;
    /**
     * @format int32
     * @default 0
     */
    size: number;
  }) => queryString.stringifyUrl({ url: `/api/PlexMedia`, query });

  static getMediaDetailByIdEndpoint = (
    plexMediaId: number,
    query: {
      type: PlexMediaType;
    },
  ) => queryString.stringifyUrl({ url: `/api/PlexMedia/detail/${plexMediaId}`, query });

  static searchPlexMediaEndpoint = (query: { query: string }) =>
    queryString.stringifyUrl({ url: `/api/PlexMedia/search`, query });
}
