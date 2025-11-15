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
  ConfigureRadarrIntegrationRequest,
  ConfigureSonarrIntegrationRequest,
  TestConnectionToRadarrEndpointResponse,
  TestConnectionToSonarrEndpointResponse,
} from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class Integration {
  /**
   * No description
   * * @tags Integration
   * @name ConfigureRadarrIntegrationEndpoint
   * @request POST:/api/Integration/Radarr/Configure
   * @secure
   */
  configureRadarrIntegrationEndpoint = (
    data: ConfigureRadarrIntegrationRequest,
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/Integration/Radarr/Configure`,
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
   * * @tags Integration
   * @name TestConnectionToRadarrEndpoint
   * @request GET:/api/Integration/Radarr/TestConnection
   * @secure
   */
  testConnectionToRadarrEndpoint = (
    query: {
      apiKey: string;
      url: string;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<TestConnectionToRadarrEndpointResponse>({
        url: `/api/Integration/Radarr/TestConnection`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<TestConnectionToRadarrEndpointResponse>);

  /**
   * No description
   * * @tags Integration
   * @name ConfigureSonarrIntegrationEndpoint
   * @request POST:/api/Integration/Sonarr/Configure
   * @secure
   */
  configureSonarrIntegrationEndpoint = (
    data: ConfigureSonarrIntegrationRequest,
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/Integration/Sonarr/Configure`,
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
   * * @tags Integration
   * @name TestConnectionToSonarrEndpoint
   * @request GET:/api/Integration/Sonarr/TestConnection
   * @secure
   */
  testConnectionToSonarrEndpoint = (
    query: {
      apiKey: string;
      url: string;
    },
    params: RequestParams = {},
  ) =>
    from(
      Axios.request<TestConnectionToSonarrEndpointResponse>({
        url: `/api/Integration/Sonarr/TestConnection`,
        method: "GET",
        params: query,
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<TestConnectionToSonarrEndpointResponse>);
}

export class IntegrationPaths {
  static configureRadarrIntegrationEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Integration/Radarr/Configure` });

  static testConnectionToRadarrEndpoint = (query: {
    apiKey: string;
    url: string;
  }) =>
    queryString.stringifyUrl({
      url: `/api/Integration/Radarr/TestConnection`,
      query,
    });

  static configureSonarrIntegrationEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Integration/Sonarr/Configure` });

  static testConnectionToSonarrEndpoint = (query: {
    apiKey: string;
    url: string;
  }) =>
    queryString.stringifyUrl({
      url: `/api/Integration/Sonarr/TestConnection`,
      query,
    });
}
