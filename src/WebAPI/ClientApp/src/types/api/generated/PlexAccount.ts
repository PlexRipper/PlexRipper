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
  BaseResultDTO,
  GeneratePlexTokenResponse,
  PlexAccountDTO,
  ValidatePlexAccountResponse,
} from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class PlexAccount {
  /**
   * No description
   * * @tags Plexaccount
   * @name CreatePlexAccountEndpoint
   * @request POST:/api/PlexAccount
   * @secure
   */
  createPlexAccountEndpoint = (data: PlexAccountDTO, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/PlexAccount`,
        method: "POST",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexaccount
   * @name GetAllPlexAccountsEndpoint
   * @request GET:/api/PlexAccount
   * @secure
   */
  getAllPlexAccountsEndpoint = (
    query?: {
      /** @default false */
      enabledOnly?: boolean;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<PlexAccountDTO[]>({
        url: `/api/PlexAccount`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexAccountDTO[]>);

  /**
   * No description
   * * @tags Plexaccount
   * @name UpdatePlexAccountByIdEndpoint
   * @request PUT:/api/PlexAccount
   * @secure
   */
  updatePlexAccountByIdEndpoint = (data: PlexAccountDTO, params: RequestParams = {}) =>
    from(
      Axios.request<PlexAccountDTO>({
        url: `/api/PlexAccount`,
        method: "PUT",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexAccountDTO>);

  /**
   * No description
   * * @tags Plexaccount
   * @name DeletePlexAccountByIdEndpoint
   * @request DELETE:/api/PlexAccount/{PlexAccountId}
   * @secure
   */
  deletePlexAccountByIdEndpoint = (plexAccountId: number, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/PlexAccount/${plexAccountId}`,
        method: "DELETE",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexaccount
   * @name GetPlexAccountByIdEndpoint
   * @request GET:/api/PlexAccount/{PlexAccountId}
   * @secure
   */
  getPlexAccountByIdEndpoint = (plexAccountId: number, params: RequestParams = {}) =>
    from(
      Axios.request<PlexAccountDTO>({
        url: `/api/PlexAccount/${plexAccountId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexAccountDTO>);

  /**
   * No description
   * * @tags Plexaccount
   * @name GeneratePlexTokenEndpoint
   * @request GET:/api/PlexAccount/generate-token/{PlexAccountId}
   * @secure
   */
  generatePlexTokenEndpoint = (
    plexAccountId: number,
    query?: {
      /** @default "" */
      verificationCode?: string;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<GeneratePlexTokenResponse>({
        url: `/api/PlexAccount/generate-token/${plexAccountId}`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<GeneratePlexTokenResponse>);

  /**
   * No description
   * * @tags Plexaccount
   * @name IsUsernameAvailableEndpoint
   * @request GET:/api/PlexAccount/check
   * @secure
   */
  isUsernameAvailableEndpoint = (
    query: {
      username: string;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<Boolean>({
        url: `/api/PlexAccount/check`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<Boolean>);

  /**
   * No description
   * * @tags Plexaccount
   * @name RefreshPlexAccountAccessEndpoint
   * @request GET:/api/PlexAccount/refresh/{PlexAccountId}
   * @secure
   */
  refreshPlexAccountAccessEndpoint = (plexAccountId: number, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/PlexAccount/refresh/${plexAccountId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexaccount
   * @name ValidatePlexAccountEndpoint
   * @request POST:/api/PlexAccount/validate
   * @secure
   */
  validatePlexAccountEndpoint = (data: PlexAccountDTO, params: RequestParams = {}) =>
    from(
      Axios.request<ValidatePlexAccountResponse>({
        url: `/api/PlexAccount/validate`,
        method: "POST",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ValidatePlexAccountResponse>);
}

export class PlexAccountPaths {
  static createPlexAccountEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexAccount` });

  static getAllPlexAccountsEndpoint = (query?: {
    /** @default false */
    enabledOnly?: boolean;
  }) => queryString.stringifyUrl({ url: `/api/PlexAccount`, query });

  static updatePlexAccountByIdEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexAccount` });

  static deletePlexAccountByIdEndpoint = (plexAccountId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexAccount/${plexAccountId}` });

  static getPlexAccountByIdEndpoint = (plexAccountId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexAccount/${plexAccountId}` });

  static generatePlexTokenEndpoint = (
    plexAccountId: number,
    query?: {
      /** @default "" */
      verificationCode?: string;
    },
  ) => queryString.stringifyUrl({ url: `/api/PlexAccount/generate-token/${plexAccountId}`, query });

  static isUsernameAvailableEndpoint = (query: { username: string }) =>
    queryString.stringifyUrl({ url: `/api/PlexAccount/check`, query });

  static refreshPlexAccountAccessEndpoint = (plexAccountId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexAccount/refresh/${plexAccountId}` });

  static validatePlexAccountEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexAccount/validate` });
}
