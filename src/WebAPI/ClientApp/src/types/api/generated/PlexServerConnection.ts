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
  CreatePlexServerConnectionEndpointRequest,
  PlexServerConnectionDTO,
  PlexServerStatusDTO,
  ResultDTO,
  ServerIdentityDTO,
  UpdatePlexServerConnectionEndpointRequest,
  ValidatePlexServerConnectionEndpointRequest,
} from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class PlexServerConnection {
  /**
   * No description
   * * @tags Plexserverconnection
   * @name CheckAllConnectionsStatusByPlexServerEndpoint
   * @request GET:/api/PlexServerConnection/check/by-server/{PlexServerId}
   * @secure
   */
  checkAllConnectionsStatusByPlexServerEndpoint = (plexServerId: number, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerStatusDTO[]>({
        url: `/api/PlexServerConnection/check/by-server/${plexServerId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerStatusDTO[]>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name CheckConnectionStatusByIdEndpoint
   * @request GET:/api/PlexServerConnection/check/{PlexServerConnectionId}
   * @secure
   */
  checkConnectionStatusByIdEndpoint = (plexServerConnectionId: number, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerStatusDTO>({
        url: `/api/PlexServerConnection/check/${plexServerConnectionId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerStatusDTO>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name CreatePlexServerConnectionEndpoint
   * @request POST:/api/PlexServerConnection
   * @secure
   */
  createPlexServerConnectionEndpoint = (data: CreatePlexServerConnectionEndpointRequest, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerConnectionDTO>({
        url: `/api/PlexServerConnection`,
        method: "POST",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerConnectionDTO>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name GetAllPlexServerConnectionsEndpoint
   * @request GET:/api/PlexServerConnection
   * @secure
   */
  getAllPlexServerConnectionsEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerConnectionDTO[]>({
        url: `/api/PlexServerConnection`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerConnectionDTO[]>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name UpdatePlexServerConnectionEndpoint
   * @request PATCH:/api/PlexServerConnection
   * @secure
   */
  updatePlexServerConnectionEndpoint = (data: UpdatePlexServerConnectionEndpointRequest, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerConnectionDTO>({
        url: `/api/PlexServerConnection`,
        method: "PATCH",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerConnectionDTO>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name DeletePlexServerConnectionById
   * @request DELETE:/api/PlexServerConnection/{PlexServerConnectionId}
   * @secure
   */
  deletePlexServerConnectionById = (plexServerConnectionId: number, params: RequestParams = {}) =>
    from(
      Axios.request<ResultDTO>({
        url: `/api/PlexServerConnection/${plexServerConnectionId}`,
        method: "DELETE",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ResultDTO>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name GetPlexServerConnectionByIdEndpoint
   * @request GET:/api/PlexServerConnection/{PlexServerConnectionId}
   * @secure
   */
  getPlexServerConnectionByIdEndpoint = (plexServerConnectionId: number, params: RequestParams = {}) =>
    from(
      Axios.request<PlexServerConnectionDTO>({
        url: `/api/PlexServerConnection/${plexServerConnectionId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<PlexServerConnectionDTO>);

  /**
   * No description
   * * @tags Plexserverconnection
   * @name ValidatePlexServerConnectionEndpoint
   * @request POST:/api/PlexServerConnection/validate
   * @secure
   */
  validatePlexServerConnectionEndpoint = (
    data: ValidatePlexServerConnectionEndpointRequest,
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<ServerIdentityDTO>({
        url: `/api/PlexServerConnection/validate`,
        method: "POST",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<ServerIdentityDTO>);
}

export class PlexServerConnectionPaths {
  static checkAllConnectionsStatusByPlexServerEndpoint = (plexServerId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServerConnection/check/by-server/${plexServerId}` });

  static checkConnectionStatusByIdEndpoint = (plexServerConnectionId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServerConnection/check/${plexServerConnectionId}` });

  static createPlexServerConnectionEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexServerConnection` });

  static getAllPlexServerConnectionsEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexServerConnection` });

  static updatePlexServerConnectionEndpoint = () => queryString.stringifyUrl({ url: `/api/PlexServerConnection` });

  static deletePlexServerConnectionById = (plexServerConnectionId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServerConnection/${plexServerConnectionId}` });

  static getPlexServerConnectionByIdEndpoint = (plexServerConnectionId: number) =>
    queryString.stringifyUrl({ url: `/api/PlexServerConnection/${plexServerConnectionId}` });

  static validatePlexServerConnectionEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/PlexServerConnection/validate` });
}
