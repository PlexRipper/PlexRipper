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

import { apiCheckPipe, axiosObservable } from "@api/base";
import queryString from "query-string";

export class Integration {
  /**
   * No description
   * * @tags Integration
   * @name ClearRadarrConfigurationEndpoint
   * @request DELETE:/api/Integration/Radarr/Configuration
   * @secure
   */
  clearRadarrConfigurationEndpoint = (params: RequestParams = {}) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Integration/Radarr/Configuration`,
      method: "DELETE",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

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
    axiosObservable<BaseResultDTO>({
      url: `/api/Integration/Radarr/Configure`,
      method: "POST",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

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
    axiosObservable<TestConnectionToRadarrEndpointResponse>({
      url: `/api/Integration/Radarr/TestConnection`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<TestConnectionToRadarrEndpointResponse>);

  /**
   * No description
   * * @tags Integration
   * @name ClearSonarrConfigurationEndpoint
   * @request DELETE:/api/Integration/Sonarr/Configuration
   * @secure
   */
  clearSonarrConfigurationEndpoint = (params: RequestParams = {}) =>
    axiosObservable<BaseResultDTO>({
      url: `/api/Integration/Sonarr/Configuration`,
      method: "DELETE",
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

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
    axiosObservable<BaseResultDTO>({
      url: `/api/Integration/Sonarr/Configure`,
      method: "POST",
      data: data,
      secure: true,
      type: ContentType.Json,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<BaseResultDTO>);

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
    axiosObservable<TestConnectionToSonarrEndpointResponse>({
      url: `/api/Integration/Sonarr/TestConnection`,
      method: "GET",
      params: query,
      secure: true,
      responseType: "json",
      ...params,
    }).pipe(apiCheckPipe<TestConnectionToSonarrEndpointResponse>);
}

export class IntegrationPaths {
  static clearRadarrConfigurationEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Integration/Radarr/Configuration` });

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

  static clearSonarrConfigurationEndpoint = () =>
    queryString.stringifyUrl({ url: `/api/Integration/Sonarr/Configuration` });

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
