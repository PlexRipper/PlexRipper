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

import { ErrorResponse, NotificationDTO, ResultDTO } from './data-contracts';
import { HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class Notification<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Notification
 * @name ClearAllNotificationsEndpoint
 * @request DELETE:/api/Notification/clear

 */
	clearAllNotificationsEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<number, ResultDTO>({
				path: `/api/Notification/clear`,
				method: 'DELETE',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<number>);
	/**
 * No description
 *
 * @tags Notification
 * @name GetAllNotificationsEndpoint
 * @request GET:/api/Notification/

 */
	getAllNotificationsEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<NotificationDTO[], ResultDTO>({
				path: `/api/Notification/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<NotificationDTO[]>);
	/**
 * No description
 *
 * @tags Notification
 * @name HideNotificationEndpoint
 * @request PUT:/api/Notification/{notificationId}

 */
	hideNotificationEndpoint = (notificationId: number, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/Notification/${notificationId}`,
				method: 'PUT',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
