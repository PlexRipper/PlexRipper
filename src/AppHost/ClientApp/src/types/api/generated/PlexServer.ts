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

import type {
  BaseResultDTO,
  PlexServerDTO,
  SetServerEnabledRequest,
  SetServerOwnedRequest,
} from "./data-contracts";

import { apiCheckPipe, axiosObservable } from "@api/base";
import queryString from "query-string";

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
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name GetPlexServerByIdEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}
   * @secure
   */
  getPlexServerByIdEndpoint = (
    plexServerId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexServerDTO>({
      url: `/api/PlexServer/${plexServerId}`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexServerDTO>);

  /**
   * @description  Retrieves all the PlexServers, without PlexLibraries but with all its connections currently in the database.
   * * @tags Plexserver
   * @name GetAllPlexServersEndpoint
   * @summary Get All the PlexServers, without PlexLibraries but with all its connections.
   * @request GET:/api/PlexServer
   * @secure
   */
  getAllPlexServersEndpoint = (params: RequestParams = {}) =>
    axiosObservable<PlexServerDTO[]>({
      url: `/api/PlexServer`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexServerDTO[]>);

  /**
   * No description
   * * @tags Plexserver
   * @name QueueInspectPlexServerJobEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/inspect
   * @secure
   */
  queueInspectPlexServerJobEndpoint = (
    plexServerId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexServer/${plexServerId}/inspect`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name RefreshPlexServerConnectionsEndpoint
   * @request GET:/api/PlexServer/{PlexServerId}/refresh
   * @secure
   */
  refreshPlexServerConnectionsEndpoint = (
    plexServerId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexServerDTO>({
      url: `/api/PlexServer/${plexServerId}/refresh`,
      method: "GET",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexServerDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name PausePlexServerDownloadsEndpoint
   * @request PUT:/api/PlexServer/server/pause/{PlexServerId}
   * @secure
   */
  pausePlexServerDownloadsEndpoint = (
    plexServerId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexServer/server/pause/${plexServerId}`,
      method: "PUT",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name ResumePlexServerDownloadsEndpoint
   * @request PUT:/api/PlexServer/server/resume/{PlexServerId}
   * @secure
   */
  resumePlexServerDownloadsEndpoint = (
    plexServerId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexServer/server/resume/${plexServerId}`,
      method: "PUT",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

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
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexServer/${plexServerId}/set-server-alias`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name SetServerEnabledRequestEndpoint
   * @request PUT:/api/PlexServer/{PlexServerId}/set-server-enabled
   * @secure
   */
  setServerEnabledRequestEndpoint = (
    plexServerId: number,
    data: SetServerEnabledRequest,
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexServerDTO>({
      url: `/api/PlexServer/${plexServerId}/set-server-enabled`,
      method: "PUT",
      data: data,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexServerDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name SetServerOwnedEndpoint
   * @request PUT:/api/PlexServer/{PlexServerId}/set-server-owned
   * @secure
   */
  setServerOwnedEndpoint = (
    plexServerId: number,
    data: SetServerOwnedRequest,
    params: RequestParams = {},
  ) =>
    axiosObservable<PlexServerDTO>({
      url: `/api/PlexServer/${plexServerId}/set-server-owned`,
      method: "PUT",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<PlexServerDTO>);

  /**
   * No description
   * * @tags Plexserver
   * @name SyncPlexServerMediaEndpoint
   * @request POST:/api/PlexServer/{PlexServerId}/sync
   * @secure
   */
  syncPlexServerMediaEndpoint = (
    plexServerId: number,
    params: RequestParams = {},
  ) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/PlexServer/${plexServerId}/sync`,
      method: "POST",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);
}

export class PlexServerPaths {
  static setPreferredPlexServerConnectionEndpoint = (
    plexServerId: number,
    plexServerConnectionId: number,
  ) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}`,
    });

  static getPlexServerByIdEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}` });

  static getAllPlexServersEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/PlexServer` });

  static queueInspectPlexServerJobEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/${plexServerId}/inspect`,
    });

  static refreshPlexServerConnectionsEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/${plexServerId}/refresh`,
    });

  static pausePlexServerDownloadsEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/server/pause/${plexServerId}`,
    });

  static resumePlexServerDownloadsEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/server/resume/${plexServerId}`,
    });

  static setServerAlias = (
    plexServerId: number,
    query: {
      serverAlias: string;
    },
  ) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/${plexServerId}/set-server-alias`,
      query,
    });

  static setServerEnabledRequestEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/${plexServerId}/set-server-enabled`,
    });

  static setServerOwnedEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({
      url: `/api/PlexServer/${plexServerId}/set-server-owned`,
    });

  static syncPlexServerMediaEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServer/${plexServerId}/sync` });
}
