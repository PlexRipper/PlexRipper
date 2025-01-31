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
import { ContentType } from "./http-client";

import type { BaseResultDTO, FileSystemDTO, FolderPathDTO } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class FolderPath {
  /**
   * No description
   * * @tags Folderpath
   * @name CreateFolderPathEndpoint
   * @request POST:/api/FolderPath
   * @secure
   */
  createFolderPathEndpoint = (data: FolderPathDTO, params: RequestParams = {}) =>
    from(
      Axios.request<FolderPathDTO>({
        url: `/api/FolderPath`,
        method: "POST",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<FolderPathDTO>);

  /**
   * No description
   * * @tags Folderpath
   * @name GetAllFolderPathsEndpoint
   * @request GET:/api/FolderPath
   * @secure
   */
  getAllFolderPathsEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<FolderPathDTO[]>({
        url: `/api/FolderPath`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<FolderPathDTO[]>);

  /**
   * No description
   * * @tags Folderpath
   * @name UpdateFolderPathEndpoint
   * @request PUT:/api/FolderPath
   * @secure
   */
  updateFolderPathEndpoint = (data: FolderPathDTO, params: RequestParams = {}) =>
    from(
      Axios.request<FolderPathDTO>({
        url: `/api/FolderPath`,
        method: "PUT",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<FolderPathDTO>);

  /**
   * No description
   * * @tags Folderpath
   * @name DeleteFolderPathEndpoint
   * @request DELETE:/api/FolderPath/{Id}
   * @secure
   */
  deleteFolderPathEndpoint = (id: number, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/FolderPath/${id}`,
        method: "DELETE",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Folderpath
   * @name GetFolderPathDirectoryEndpoint
   * @summary Get all the FolderPaths entities in the database
   * @request GET:/api/FolderPath/directory
   * @secure
   */
  getFolderPathDirectoryEndpoint = (
    query?: {
      /** @default "" */
      path?: string;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<FileSystemDTO>({
        url: `/api/FolderPath/directory`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<FileSystemDTO>);
}

export class FolderPathPaths {
  static createFolderPathEndpoint = () => queryString.stringifyUrl({ url: `/api/FolderPath` });

  static getAllFolderPathsEndpoint = () => queryString.stringifyUrl({ url: `/api/FolderPath` });

  static updateFolderPathEndpoint = () => queryString.stringifyUrl({ url: `/api/FolderPath` });

  static deleteFolderPathEndpoint = (id: number) => queryString.stringifyUrl({ url: `/api/FolderPath/${id}` });

  static getFolderPathDirectoryEndpoint = (query?: {
    /** @default "" */
    path?: string;
  }) => queryString.stringifyUrl({ url: `/api/FolderPath/directory`, query });
}
