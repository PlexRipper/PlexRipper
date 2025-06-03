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

import type {
  AppCredentialsDTO,
  AppUserLoginEndpointRequest,
  BaseResultDTO,
  UpdateCredentialsEndpointRequest,
  UserClaimsDTO,
} from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class Authentication {
  /**
   * No description
   * * @tags Authentication
   * @name AuthenticationStatusEndpoint
   * @request GET:/api/Authentication/status
   * @secure
   */
  authenticationStatusEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<UserClaimsDTO>({
        url: `/api/Authentication/status`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<UserClaimsDTO>);

  /**
   * No description
   * * @tags Authentication
   * @name GetAppCredentials
   * @summary Gets the username and random password from the main App user.
   * @request GET:/api/Authentication
   * @secure
   */
  getAppCredentials = (params: RequestParams = {}) =>
    from(
      Axios.request<AppCredentialsDTO>({
        url: `/api/Authentication`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<AppCredentialsDTO>);

  /**
   * No description
   * * @tags Authentication
   * @name UpdateCredentialsEndpoint
   * @summary Updates the credentials of a user.
   * @request PUT:/api/Authentication
   * @secure
   */
  updateCredentialsEndpoint = (data: UpdateCredentialsEndpointRequest, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/Authentication`,
        method: "PUT",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * @description Logs in a user.
   * * @tags Authentication
   * @name AppUserLoginEndpoint
   * @summary User Login
   * @request POST:/api/Authentication/login
   * @secure
   */
  appUserLoginEndpoint = (data: AppUserLoginEndpointRequest, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/Authentication/login`,
        method: "POST",
        data: data,
        secure: true,
        type: ContentType.FormData,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Authentication
   * @name AppUserLogOutEndpoint
   * @request POST:/api/Authentication/logout
   * @secure
   */
  appUserLogOutEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<String>({
        url: `/api/Authentication/logout`,
        method: "POST",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<String>);
}

export class AuthenticationPaths {
  static authenticationStatusEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/status` });

  static getAppCredentials = () => queryString.stringifyUrl({ url: `/api/Authentication` });

  static updateCredentialsEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication` });

  static appUserLoginEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/login` });

  static appUserLogOutEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/logout` });
}
