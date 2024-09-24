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

import type { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class PlexMedia {
  /**
 * No description
 *
 * @tags Plexmedia
 * @name GetMediaDetailByIdEndpoint
 * @request GET:/api/PlexMedia/detail/{plexMediaId}

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
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexMediaDTO>);

  /**
 * No description
 *
 * @tags Plexmedia
 * @name SearchPlexMediaEndpoint
 * @request GET:/api/PlexMedia/search

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
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexMediaSlimDTO[]>);
}

export class PlexMediaPaths {
  static getMediaDetailByIdEndpoint = (
    plexMediaId: number,
    query: {
      type: PlexMediaType;
    },
  ) => queryString.stringifyUrl({ url: `/api/PlexMedia/detail/${plexMediaId}`, query });

  static searchPlexMediaEndpoint = (query: { query: string }) =>
    queryString.stringifyUrl({ url: `/api/PlexMedia/search`, query });
}
