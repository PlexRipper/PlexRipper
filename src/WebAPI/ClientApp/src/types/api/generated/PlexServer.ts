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

import { ErrorResponse, PlexServerDTO, ResultDTO } from './data-contracts';
import { HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class PlexServer<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Plexserver
 * @name SetPreferredPlexServerConnectionEndpoint
 * @request GET:/api/PlexServer/{plexServerId}/preferred-connection/{plexServerConnectionId}

 */
	setPreferredPlexServerConnectionEndpoint = (
		plexServerId: number,
		plexServerConnectionId: number,
		params: RequestParams = {},
	) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Plexserver
 * @name GetPlexServerByIdEndpoint
 * @request GET:/api/PlexServer/{plexServerId}

 */
	getPlexServerByIdEndpoint = (plexServerId: number, params: RequestParams = {}) =>
		from(
			this.request<PlexServerDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexServer/${plexServerId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerDTO>);
	/**
 * @description  Retrieves all the PlexServers, without PlexLibraries but with all its connections currently in the database.
 *
 * @tags Plexserver
 * @name GetAllPlexServersEndpoint
 * @summary Get All the PlexServers, without PlexLibraries but with all its connections.
 * @request GET:/api/PlexServer/

 */
	getAllPlexServersEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<PlexServerDTO[], ResultDTO>({
				path: `/api/PlexServer/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerDTO[]>);
	/**
 * No description
 *
 * @tags Plexserver
 * @name QueueInspectPlexServerJobEndpoint
 * @request GET:/api/PlexServer/{plexServerId}/inspect

 */
	queueInspectPlexServerJobEndpoint = (plexServerId: number, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/inspect`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Plexserver
 * @name RefreshPlexServerConnectionsEndpoint
 * @request GET:/api/PlexServer/{plexServerId}/refresh

 */
	refreshPlexServerConnectionsEndpoint = (plexServerId: number, params: RequestParams = {}) =>
		from(
			this.request<PlexServerDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/refresh`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerDTO>);
	/**
 * No description
 *
 * @tags Plexserver
 * @name QueueSyncPlexServerJobEndpoint
 * @request GET:/api/PlexServer/{plexServerId}/sync

 */
	queueSyncPlexServerJobEndpoint = (
		plexServerId: number,
		query?: {
			/** @default false */
			forceSync?: boolean;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/sync`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
