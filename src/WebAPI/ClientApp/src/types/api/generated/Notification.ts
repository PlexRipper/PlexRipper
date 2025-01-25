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

import type { BaseResultDTO, NotificationDTO, SetNotificationVisibilityEndpointRequest } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class Notification {
  /**
   * No description
   * * @tags Notification
   * @name ClearAllNotificationsEndpoint
   * @request DELETE:/api/Notification/clear
   * @secure
   */
  clearAllNotificationsEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<number>({
        url: `/api/Notification/clear`,
        method: "DELETE",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<number>);

  /**
   * No description
   * * @tags Notification
   * @name GetAllNotificationsEndpoint
   * @request GET:/api/Notification
   * @secure
   */
  getAllNotificationsEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<NotificationDTO[]>({
        url: `/api/Notification`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<NotificationDTO[]>);

  /**
   * No description
   * * @tags Notification
   * @name SetNotificationVisibilityEndpoint
   * @request PATCH:/api/Notification
   * @secure
   */
  setNotificationVisibilityEndpoint = (data: SetNotificationVisibilityEndpointRequest, params: RequestParams = {}) =>
    from(
      Axios.request<BaseResultDTO>({
        url: `/api/Notification`,
        method: "PATCH",
        data: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<BaseResultDTO>);
}

export class NotificationPaths {
  static clearAllNotificationsEndpoint = () => queryString.stringifyUrl({ url: `/api/Notification/clear` });

  static getAllNotificationsEndpoint = () => queryString.stringifyUrl({ url: `/api/Notification` });

  static setNotificationVisibilityEndpoint = () => queryString.stringifyUrl({ url: `/api/Notification` });
}
