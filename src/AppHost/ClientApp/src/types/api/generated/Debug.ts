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

import type { PlexMediaType } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class Debug {
  /**
   * No description
   * * @tags Debug
   * @name GetAllSubtitlesEndpoint
   * @request GET:/api/Debug/get-all-subtitles
   * @secure
   */
  getAllSubtitlesEndpoint = (
    query: {
      /** @format int32 */
      count?: number | null;
      type: PlexMediaType;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<String[]>({
        url: `/api/Debug/get-all-subtitles`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<String[]>);

  /**
   * No description
   * * @tags Debug
   * @name GetAllUniqueMediaTitlesEndpoint
   * @request GET:/api/Debug/unique-media-titles
   * @secure
   */
  getAllUniqueMediaTitlesEndpoint = (
    query: {
      /** @format int32 */
      count: number;
      type: PlexMediaType;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<String[]>({
        url: `/api/Debug/unique-media-titles`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<String[]>);
}

export class DebugPaths {
  static getAllSubtitlesEndpoint = (query: {
    /** @format int32 */
    count?: number | null;
    type: PlexMediaType;
  }) => queryString.stringifyUrl({ url: `/api/Debug/get-all-subtitles`, query });

  static getAllUniqueMediaTitlesEndpoint = (query: {
    /** @format int32 */
    count: number;
    type: PlexMediaType;
  }) => queryString.stringifyUrl({ url: `/api/Debug/unique-media-titles`, query });
}
