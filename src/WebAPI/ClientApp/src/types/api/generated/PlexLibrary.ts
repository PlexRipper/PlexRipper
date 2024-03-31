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

import { ErrorResponse, PlexLibraryDTO, PlexMediaSlimDTO, ResultDTO } from './data-contracts';
import { HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class PlexLibrary<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Plexlibrary
 * @name GetPlexLibraryByIdEndpoint
 * @request GET:/api/PlexLibrary/{plexLibraryId}

 */
	getPlexLibraryByIdEndpoint = (plexLibraryId: number, params: RequestParams = {}) =>
		from(
			this.request<PlexLibraryDTO, ResultDTO>({
				path: `/api/PlexLibrary/${plexLibraryId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexLibraryDTO>);
	/**
 * No description
 *
 * @tags Plexlibrary
 * @name GetAllPlexLibrariesEndpoint
 * @request GET:/api/PlexLibrary/

 */
	getAllPlexLibrariesEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<PlexLibraryDTO[], ResultDTO>({
				path: `/api/PlexLibrary/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexLibraryDTO[]>);
	/**
 * No description
 *
 * @tags Plexlibrary
 * @name GetPlexLibraryMediaEndpoint
 * @request GET:/api/PlexLibrary/{plexLibraryId}/media

 */
	getPlexLibraryMediaEndpoint = (
		plexLibraryId: number,
		query: {
			/**
			 * @format int32
			 * @default 0
			 */
			page: number;
			/**
			 * @format int32
			 * @default 0
			 */
			size: number;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<PlexMediaSlimDTO[], ResultDTO>({
				path: `/api/PlexLibrary/${plexLibraryId}/media`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexMediaSlimDTO[]>);
	/**
 * No description
 *
 * @tags Plexlibrary
 * @name RefreshLibraryMediaEndpoint
 * @request GET:/api/PlexLibrary/refresh/{plexLibraryId}

 */
	refreshLibraryMediaEndpoint = (plexLibraryId: number, params: RequestParams = {}) =>
		from(
			this.request<PlexLibraryDTO, ResultDTO>({
				path: `/api/PlexLibrary/refresh/${plexLibraryId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexLibraryDTO>);
	/**
 * No description
 *
 * @tags Plexlibrary
 * @name SetPlexLibraryDefaultDestinationByIdEndpoint
 * @request PUT:/api/PlexLibrary/{plexLibraryId}/default/destination/{folderPathId}

 */
	setPlexLibraryDefaultDestinationByIdEndpoint = (plexLibraryId: number, folderPathId: number, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexLibrary/${plexLibraryId}/default/destination/${folderPathId}`,
				method: 'PUT',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
}
