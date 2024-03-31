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

import { ResultDTO, SettingsModelDTO } from './data-contracts';
import { ContentType, HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class Settings<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Settings
 * @name GetUserSettingsEndpoint
 * @request GET:/api/Settings/

 */
	getUserSettingsEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<SettingsModelDTO, ResultDTO>({
				path: `/api/Settings/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<SettingsModelDTO>);
	/**
 * No description
 *
 * @tags Settings
 * @name UpdateUserSettingsEndpoint
 * @request PUT:/api/Settings/

 */
	updateUserSettingsEndpoint = (data: SettingsModelDTO, params: RequestParams = {}) =>
		from(
			this.request<SettingsModelDTO, ResultDTO>({
				path: `/api/Settings/`,
				method: 'PUT',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<SettingsModelDTO>);
	/**
 * No description
 *
 * @tags Settings
 * @name ResetDatabaseEndpoint
 * @request GET:/api/Settings/resetdb

 */
	resetDatabaseEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Settings/resetdb`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
