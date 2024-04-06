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
import { ContentType, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import queryString from 'query-string';
import { from } from 'rxjs';

export class Settings {
	/**
 * No description
 *
 * @tags Settings
 * @name GetUserSettingsEndpoint
 * @request GET:/api/Settings/

 */
	getUserSettingsEndpoint = (params: RequestParams = {}) =>
		from(
			Axios.request<SettingsModelDTO>({
				url: `/api/Settings/`,
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
			Axios.request<SettingsModelDTO>({
				url: `/api/Settings/`,
				method: 'PUT',
				data: data,
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
			Axios.request<ResultDTO>({
				url: `/api/Settings/resetdb`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}

export class SettingsPaths {
	static getUserSettingsEndpoint = () => queryString.stringifyUrl({ url: `/api/Settings/` });

	static updateUserSettingsEndpoint = () => queryString.stringifyUrl({ url: `/api/Settings/` });

	static resetDatabaseEndpoint = () => queryString.stringifyUrl({ url: `/api/Settings/resetdb` });
}
