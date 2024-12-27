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

import type { PlexServerDTO, ResultDTO } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class PlexServer {
  /**
   * No description
   * * @tags Plexserver
   * @name SetPreferredPlexServerConnectionEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/preferred-connection/{PlexServerConnectionId}
   * @secure
   */
  setPreferredPlexServerConnectionEndpoint = (
    plexServerId: number,
    plexServerConnectionId: number,
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name GetPlexServerByIdEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}
   * @secure
   */
  getPlexServerByIdEndpoint = (plexServerId: number, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerDTO>({
        url: `/api/PlexServer/${plexServerId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerDTO>);

  /**
   * @description  Retrieves all the PlexServers, without PlexLibraries but with all its connections currently in the database.
   * * @tags Plexserver
   * @name GetAllPlexServersEndpoint
   * @summary Get All the PlexServers, without PlexLibraries but with all its connections.
   * @request GET:/api/PlexServer
   * @secure
   */
  getAllPlexServersEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerDTO[]>({
        url: `/api/PlexServer`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerDTO[]>);

  /**
   * No description
   * * @tags Plexserver
   * @name QueueInspectPlexServerJobEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/inspect
   * @secure
   */
  queueInspectPlexServerJobEndpoint = (plexServerId: number, params: RequestParams = {}) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/PlexServer/${plexServerId}/inspect`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name RefreshPlexServerConnectionsEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/refresh
   * @secure
   */
  refreshPlexServerConnectionsEndpoint = (plexServerId: number, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerDTO>({
        url: `/api/PlexServer/${plexServerId}/refresh`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name SetServerAlias
   * @request GET:/api/PlexServer/{PlexServerId}/set-server-alias
   * @secure
   */
  setServerAlias = (
    plexServerId: number,
    query: {
      serverAlias: string;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/PlexServer/${plexServerId}/set-server-alias`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name SetServerHiddenRequestEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/set-server-hidden
   * @secure
   */
  setServerHiddenRequestEndpoint = (
    plexServerId: number,
    query: {
      hidden: boolean;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/PlexServer/${plexServerId}/set-server-hidden`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name QueueSyncPlexServerJobEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/sync
   * @secure
   */
  queueSyncPlexServerJobEndpoint = (
    plexServerId: number,
    query?: {
      /** @default false */
      forceSync?: boolean;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/PlexServer/${plexServerId}/sync`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);
}

export class PlexServerPaths {
  static setPreferredPlexServerConnectionEndpoint = (plexServerId: number, plexServerConnectionId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}` });

  static getPlexServerByIdEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}` });

  static getAllPlexServersEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexServer` });

  static queueInspectPlexServerJobEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/inspect` });

  static refreshPlexServerConnectionsEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/refresh` });

  static setServerAlias = (
    plexServerId: number,
    query: {
      serverAlias: string;
    },
  ) => queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/set-server-alias`, query });

  static setServerHiddenRequestEndpoint = (
    plexServerId: number,
    query: {
      hidden: boolean;
    },
  ) => queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/set-server-hidden`, query });

  static queueSyncPlexServerJobEndpoint = (
    plexServerId: number,
    query?: {
      /** @default false */
      forceSync?: boolean;
    },
  ) => queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/sync`, query });
}
