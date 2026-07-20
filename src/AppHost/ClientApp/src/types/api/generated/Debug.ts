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
  LiveLogEventDTO,
  MovieLibraryComparisonDebugResponseDTO,
  PlexMediaType,
  TvShowLibraryComparisonDebugResponseDTO,
} from "./data-contracts";

import { apiCheckPipe, axiosObservable } from "@api/base";
import queryString from "query-string";

export class Debug {
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
    axiosObservable<String[]>({
      url: `/api/Debug/unique-media-titles`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<String[]>);

  /**
   * No description
   * * @tags Debug
   * @name GetAllLogsEndpoint
   * @request GET:/api/Debug/logs
   * @secure
   */
  getAllLogsEndpoint = (params: RequestParams = {}) =>
    axiosObservable<LiveLogEventDTO[]>({
      url: `/api/Debug/logs`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<LiveLogEventDTO[]>);

  /**
   * No description
   * * @tags Debug
   * @name GetMovieLibraryComparisonDebugEndpoint
   * @request GET:/api/Debug/movie-library-comparison
   * @secure
   */
  getMovieLibraryComparisonDebugEndpoint = (
    query: {
      /** @format int32 */
      ownedLibraryId: number;
      /** @format int32 */
      remoteLibraryId: number;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<MovieLibraryComparisonDebugResponseDTO>({
      url: `/api/Debug/movie-library-comparison`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<MovieLibraryComparisonDebugResponseDTO>);

  /**
   * No description
   * * @tags Debug
   * @name GetTvShowLibraryComparisonDebugEndpoint
   * @request GET:/api/Debug/tv-show-library-comparison
   * @secure
   */
  getTvShowLibraryComparisonDebugEndpoint = (
    query: {
      /** @format int32 */
      ownedLibraryId: number;
      /** @format int32 */
      remoteLibraryId: number;
    },
    params: RequestParams = {},
  ) =>
    axiosObservable<TvShowLibraryComparisonDebugResponseDTO>({
      url: `/api/Debug/tv-show-library-comparison`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<TvShowLibraryComparisonDebugResponseDTO>);
}

export class DebugPaths {
  static getAllUniqueMediaTitlesEndpoint = (query: {
    /** @format int32 */
    count: number;
    type: PlexMediaType;
  }) =>
    queryString.stringifyUrl({ url: `/api/Debug/unique-media-titles`, query });

  static getAllLogsEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Debug/logs` });

  static getMovieLibraryComparisonDebugEndpoint = (query: {
    /** @format int32 */
    ownedLibraryId: number;
    /** @format int32 */
    remoteLibraryId: number;
  }) =>
    queryString.stringifyUrl({
      url: `/api/Debug/movie-library-comparison`,
      query,
    });

  static getTvShowLibraryComparisonDebugEndpoint = (query: {
    /** @format int32 */
    ownedLibraryId: number;
    /** @format int32 */
    remoteLibraryId: number;
  }) =>
    queryString.stringifyUrl({
      url: `/api/Debug/tv-show-library-comparison`,
      query,
    });
}
