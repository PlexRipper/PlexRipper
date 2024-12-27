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

import type { AppUserLoginEndpointRequest, ResultDTO, UserClaimsDTO } from "./data-contracts";

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
   */
  authenticationStatusEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<UserClaimsDTO>({
        url: `/api/Authentication/status`,
        method: "GET",
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<UserClaimsDTO>);

  /**
   * @description Logs in a user.
   * * @tags Authentication
   * @name AppUserLoginEndpoint
   * @summary User Login
   * @request POST:/api/Authentication/login
   */
  appUserLoginEndpoint = (data: AppUserLoginEndpointRequest, params: RequestParams = {}) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/Authentication/login`,
        method: "POST",
        data: data,
        type: ContentType.FormData,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);

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

  static appUserLoginEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/login` });

  static appUserLogOutEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/logout` });
}
