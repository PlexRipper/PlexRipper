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

import { PlexServerDTO, ResultDTO } from './data-contracts';
import { RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import { from } from 'rxjs';

export class PlexServer {
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
			Axios.request<ResultDTO>({
				url: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}`,
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
			Axios.request<PlexServerDTO>({
				url: `/api/PlexServer/${plexServerId}`,
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
			Axios.request<PlexServerDTO[]>({
				url: `/api/PlexServer/`,
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
			Axios.request<ResultDTO>({
				url: `/api/PlexServer/${plexServerId}/inspect`,
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
			Axios.request<PlexServerDTO>({
				url: `/api/PlexServer/${plexServerId}/refresh`,
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
			Axios.request<ResultDTO>({
				url: `/api/PlexServer/${plexServerId}/sync`,
				method: 'GET',
				params: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
