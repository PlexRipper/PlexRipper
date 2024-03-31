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

import { Blob, PlexMediaDTO, PlexMediaType, ResultDTO } from './data-contracts';
import { HttpClient, RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import { from } from 'rxjs';

export class PlexMedia<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
 * No description
 *
 * @tags Plexmedia
 * @name GetMediaDetailByIdEndpoint
 * @request GET:/api/PlexMedia/detail/{plexMediaId}

 */
	getMediaDetailByIdEndpoint = (
		plexMediaId: number,
		query: {
			type: PlexMediaType;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<PlexMediaDTO, ResultDTO>({
				path: `/api/PlexMedia/detail/${plexMediaId}`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<PlexMediaDTO>);
	/**
 * No description
 *
 * @tags Plexmedia
 * @name GetThumbnailImageEndpoint
 * @request GET:/api/PlexMedia/thumb/{mediaId}

 */
	getThumbnailImageEndpoint = (
		mediaId: number,
		query: {
			/** @format int32 */
			height: number;
			mediaType: PlexMediaType;
			/** @format int32 */
			width: number;
		},
		params: RequestParams = {},
	) =>
		from(
			this.request<Blob, ResultDTO>({
				path: `/api/PlexMedia/thumb/${mediaId}`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<Blob>);
}
