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

import { PlexServerConnectionDTO, PlexServerStatusDTO } from './data-contracts';
import { RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import { from } from 'rxjs';

export class PlexServerConnection {
	/**
 * No description
 *
 * @tags Plexserverconnection
 * @name CheckAllConnectionsStatusByPlexServerEndpoint
 * @request GET:/api/PlexServerConnection/check/by-server/{plexServerId}

 */
	checkAllConnectionsStatusByPlexServerEndpoint = (plexServerId: number, params: RequestParams = {}) =>
		from(
			Axios.request<PlexServerStatusDTO[]>({
				url: `/api/PlexServerConnection/check/by-server/${plexServerId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerStatusDTO[]>);
	/**
 * No description
 *
 * @tags Plexserverconnection
 * @name CheckConnectionStatusByIdEndpoint
 * @request GET:/api/PlexServerConnection/check/{plexServerConnectionId}

 */
	checkConnectionStatusByIdEndpoint = (plexServerConnectionId: number, params: RequestParams = {}) =>
		from(
			Axios.request<PlexServerStatusDTO>({
				url: `/api/PlexServerConnection/check/${plexServerConnectionId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerStatusDTO>);
	/**
 * No description
 *
 * @tags Plexserverconnection
 * @name GetPlexServerConnectionByIdEndpoint
 * @request GET:/api/PlexServerConnection/{plexServerConnectionId}

 */
	getPlexServerConnectionByIdEndpoint = (plexServerConnectionId: number, params: RequestParams = {}) =>
		from(
			Axios.request<PlexServerConnectionDTO>({
				url: `/api/PlexServerConnection/${plexServerConnectionId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerConnectionDTO>);
	/**
 * No description
 *
 * @tags Plexserverconnection
 * @name GetAllPlexServerConnectionsEndpoint
 * @request GET:/api/PlexServerConnection/

 */
	getAllPlexServerConnectionsEndpoint = (params: RequestParams = {}) =>
		from(
			Axios.request<PlexServerConnectionDTO[]>({
				url: `/api/PlexServerConnection/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexServerConnectionDTO[]>);
}
