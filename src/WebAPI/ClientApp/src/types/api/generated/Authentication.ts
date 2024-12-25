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

import type { AppUserLoginEndpointRequest, UserClaimsDTO } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class Authentication {
  /**
   * No description
   * * @tags Authentication
   * @name TestAuthenticatedEndpoint
   * @request GET:/api/Authentication/auth-test
   * @secure
   */
  testAuthenticatedEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<UserClaimsDTO>({
        url: `/api/Authentication/auth-test`,
        method: "GET",
        secure: true,
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
      Axios.request<void>({
        url: `/api/Authentication/login`,
        method: "POST",
        data: data,
        type: ContentType.Json,
        ...params,
      }),
    ).pipe(apiCheckPipe<void>);

  /**
   * No description
   * * @tags Authentication
   * @name AppUserLogOutEndpoint
   * @request POST:/api/Authentication/logout
   * @secure
   */
  appUserLogOutEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<void>({
        url: `/api/Authentication/logout`,
        method: "POST",
        secure: true,
        ...params,
      }),
    ).pipe(apiCheckPipe<void>);
}

export class AuthenticationPaths {
  static testAuthenticatedEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/auth-test` });

  static appUserLoginEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/login` });

  static appUserLogOutEndpoint = () => queryString.stringifyUrl({ url: `/api/Authentication/logout` });
}
