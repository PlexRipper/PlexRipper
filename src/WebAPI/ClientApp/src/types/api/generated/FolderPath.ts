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

import { FileSystemDTO, FolderPathDTO, ResultDTO } from './data-contracts';
import { ContentType, HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class FolderPath<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Folderpath
 * @name CreateFolderPathEndpoint
 * @request POST:/api/FolderPath/

 */
	createFolderPathEndpoint = (data: FolderPathDTO, params: RequestParams = {}) =>
		from(
			this.request<FolderPathDTO, ResultDTO>({
				path: `/api/FolderPath/`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<FolderPathDTO>);
	/**
 * No description
 *
 * @tags Folderpath
 * @name GetAllFolderPathsEndpoint
 * @request GET:/api/FolderPath/

 */
	getAllFolderPathsEndpoint = (params: RequestParams = {}) =>
		from(
			this.request<FolderPathDTO[], any>({
				path: `/api/FolderPath/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<FolderPathDTO[]>);
	/**
 * No description
 *
 * @tags Folderpath
 * @name UpdateFolderPathEndpoint
 * @request PUT:/api/FolderPath/

 */
	updateFolderPathEndpoint = (data: FolderPathDTO, params: RequestParams = {}) =>
		from(
			this.request<FolderPathDTO, ResultDTO>({
				path: `/api/FolderPath/`,
				method: 'PUT',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<FolderPathDTO>);
	/**
 * No description
 *
 * @tags Folderpath
 * @name DeleteFolderPathEndpoint
 * @request DELETE:/api/FolderPath/{id}

 */
	deleteFolderPathEndpoint = (id: number, params: RequestParams = {}) =>
		from(
			this.request<ResultDTO, ResultDTO>({
				path: `/api/FolderPath/${id}`,
				method: 'DELETE',
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<ResultDTO>);
	/**
 * No description
 *
 * @tags Folderpath
 * @name GetFolderPathDirectoryEndpoint
 * @summary Get all the FolderPaths entities in the database
 * @request GET:/api/FolderPath/directory

 */
	getFolderPathDirectoryEndpoint = (
		query: {
			path: string;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<FileSystemDTO, ResultDTO>({
				path: `/api/FolderPath/directory`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<FileSystemDTO>);
}
