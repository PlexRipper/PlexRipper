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

import { AuthPin, ErrorResponse, PlexAccountDTO, ResultDTO, UpdatePlexAccountDTO } from './data-contracts';
import { ContentType, HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class PlexAccount<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Plexaccount
 * @name CreatePlexAccountEndpoint
 * @request POST:/api/PlexAccount/

 */
	createPlexAccountEndpoint = (data: PlexAccountDTO, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexAccount/`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name DeletePlexAccountByIdEndpoint
 * @request DELETE:/api/PlexAccount/{plexAccountId}

 */
	deletePlexAccountByIdEndpoint = (plexAccountId: number, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexAccount/${plexAccountId}`,
				method: 'DELETE',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name GetPlexAccountByIdEndpoint
 * @request GET:/api/PlexAccount/{plexAccountId}

 */
	getPlexAccountByIdEndpoint = (plexAccountId: number, params: RequestParams = {}) =>
		from(
			this.request<PlexAccountDTO, ResultDTO>({
				path: `/api/PlexAccount/${plexAccountId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexAccountDTO>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name GetAllPlexAccountsEndpoint
 * @request GET:/api/PlexAccount

 */
	getAllPlexAccountsEndpoint = (
		query?: {
			/** @default false */
			enabledOnly?: boolean;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<PlexAccountDTO[], ResultDTO>({
				path: `/api/PlexAccount`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexAccountDTO[]>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name UpdatePlexAccountByIdEndpoint
 * @request PUT:/api/PlexAccount

 */
	updatePlexAccountByIdEndpoint = (
		query: {
			inspect: boolean;
		},
		data: UpdatePlexAccountDTO,
		params: RequestParams = {},
	) =>
		from(
			this.request<PlexAccountDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexAccount`,
				method: 'PUT',
				query: query,
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexAccountDTO>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name IsUsernameAvailableEndpoint
 * @request GET:/api/PlexAccount/check

 */
	isUsernameAvailableEndpoint = (
		query: {
			username: string;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<Boolean, ErrorResponse | ResultDTO>({
				path: `/api/PlexAccount/check`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<Boolean>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name RefreshPlexAccountAccessEndpoint
 * @request GET:/api/PlexAccount/refresh/{plexAccountId}

 */
	refreshPlexAccountAccessEndpoint = (plexAccountId: number, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexAccount/refresh/${plexAccountId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name ValidatePlexAccountEndpoint
 * @request POST:/api/PlexAccount/validate

 */
	validatePlexAccountEndpoint = (data: PlexAccountDTO, params: RequestParams = {}) =>
		from(
			this.request<PlexAccountDTO, ResultDTO>({
				path: `/api/PlexAccount/validate`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexAccountDTO>);
	/**
 * No description
 *
 * @tags Plexaccount
 * @name Verify2FaPinEndpoint
 * @request GET:/api/PlexAccount/authpin

 */
	verify2FaPinEndpoint = (
		query: {
			/**
			 * @format int32
			 * @default 0
			 */
			authPinId?: number;
			clientId: string;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<AuthPin, ResultDTO>({
				path: `/api/PlexAccount/authpin`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<AuthPin>);
}
