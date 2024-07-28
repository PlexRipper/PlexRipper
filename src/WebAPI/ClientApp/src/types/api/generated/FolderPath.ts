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

import type { RequestParams } from './http-client';
import { ContentType } from './http-client';

import type { FileSystemDTO, FolderPathDTO, ResultDTO } from './data-contracts';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import queryString from 'query-string';
import { from } from 'rxjs';

export class FolderPath {
	/**
 * No description
 *
 * @tags Folderpath
 * @name CreateFolderPathEndpoint
 * @request POST:/api/FolderPath/

 */
	createFolderPathEndpoint = (data: FolderPathDTO, params: RequestParams = {}) =>
		from(
			Axios.request<FolderPathDTO>({
				url: `/api/FolderPath/`,
				method: 'POST',
				data: data,
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
			Axios.request<FolderPathDTO[]>({
				url: `/api/FolderPath/`,
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
			Axios.request<FolderPathDTO>({
				url: `/api/FolderPath/`,
				method: 'PUT',
				data: data,
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
			Axios.request<ResultDTO>({
				url: `/api/FolderPath/${id}`,
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
			Axios.request<FileSystemDTO>({
				url: `/api/FolderPath/directory`,
				method: 'GET',
				params: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<FileSystemDTO>);
}

export class FolderPathPaths {
	static createFolderPathEndpoint = () => queryString.stringifyUrl({ url: `/api/FolderPath/` });

	static getAllFolderPathsEndpoint = () => queryString.stringifyUrl({ url: `/api/FolderPath/` });

	static updateFolderPathEndpoint = () => queryString.stringifyUrl({ url: `/api/FolderPath/` });

	static deleteFolderPathEndpoint = (id: number) => queryString.stringifyUrl({ url: `/api/FolderPath/${id}` });

	static getFolderPathDirectoryEndpoint = (query: { path: string }) =>
		queryString.stringifyUrl({ url: `/api/FolderPath/directory`, query });
}
