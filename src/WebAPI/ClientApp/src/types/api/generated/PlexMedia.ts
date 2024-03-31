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

import { Blob, PlexMediaDTO, PlexMediaType } from './data-contracts';
import { RequestParams } from './http-client';

import { apiCheckPipe } from '@api/base';
import Axios from 'axios';
import { from } from 'rxjs';

export class PlexMedia {
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
			Axios.request<PlexMediaDTO>({
				url: `/api/PlexMedia/detail/${plexMediaId}`,
				method: 'GET',
				params: query,
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
			Axios.request<Blob>({
				url: `/api/PlexMedia/thumb/${mediaId}`,
				method: 'GET',
				params: query,
				format: 'json',
				...params,
			}),
		).pipe(apiCheckPipe<Blob>);
}
