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
 *
 * @tags Debug
 * @name GetAllUniqueMediaTitlesEndpoint
 * @request GET:/api/Debug/unique-media-titles

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
      Axios.request<string[]>({
        url: `/api/Debug/unique-media-titles`,
        method: "GET",
        params: query,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<string[]>);
}

export class DebugPaths {
  static getAllUniqueMediaTitlesEndpoint = (query: {
    /** @format int32 */
    count: number;
    type: PlexMediaType;
  }) => queryString.stringifyUrl({ url: `/api/Debug/unique-media-titles`, query });
}
