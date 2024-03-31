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

import { NotificationDTO, ResultDTO } from './data-contracts';
import { RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import { from } from 'rxjs';

export class Notification {
	/**
 * No description
 *
 * @tags Notification
 * @name ClearAllNotificationsEndpoint
 * @request DELETE:/api/Notification/clear

 */
	clearAllNotificationsEndpoint = (params: RequestParams = {}) =>
		from(
			Axios.request<number>({
				url: `/api/Notification/clear`,
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
			Axios.request<NotificationDTO[]>({
				url: `/api/Notification/`,
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
			Axios.request<ResultDTO>({
				url: `/api/Notification/${notificationId}`,
				method: 'PUT',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
