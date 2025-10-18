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

import type { BaseResultDTO, SettingsModelDTO } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class Settings {
  /**
   * No description
   * * @tags Settings
   * @name GetUserSettingsEndpoint
   * @request GET:/api/Settings
   * @secure
   */
  getUserSettingsEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<SettingsModelDTO>({
        url: `/api/Settings`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<SettingsModelDTO>);

  /**
   * No description
   * * @tags Settings
   * @name UpdateUserSettingsEndpoint
   * @request PUT:/api/Settings
   * @secure
   */
  updateUserSettingsEndpoint = (
    data: SettingsModelDTO,
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<SettingsModelDTO>({
        url: `/api/Settings`,
        method: "PUT",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<SettingsModelDTO>);

  /**
   * No description
   * * @tags Settings
   * @name ResetDatabaseEndpoint
   * @request GET:/api/Settings/resetdb
   * @secure
   */
  resetDatabaseEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/Settings/resetdb`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);
}

export class SettingsPaths {
  static getUserSettingsEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Settings` });

  static updateUserSettingsEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Settings` });

  static resetDatabaseEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Settings/resetdb` });
}
