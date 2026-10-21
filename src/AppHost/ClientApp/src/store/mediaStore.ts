import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs } from 'vue';
import { from, type Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, take, catchError } from 'rxjs/operators';
import type { PlexMediaComparisonDetailsDTO, PlexMediaType, PlexMediaDTO, BaseResultDTO, PhotoAssetBrowseDTO } from '@dto';
import { PhotoImageRepresentation } from '@dto';
import { StoreNames, type ISetupResult } from '@interfaces';
import { plexMediaApi, plexPhotoApi } from '@api';
import { cloneDeep } from 'lodash-es';
import Log from 'consola';
import Axios, { type AxiosRequestConfig } from 'axios';

interface IMediaUrlStoreState {
	mediaUrls: IObjectUrl[];
}

interface IObjectUrl {
	plexServerId: number;
	plexKey: string;
	metaDataKey: number;
	url: string;
}

export const useMediaStore = defineStore(StoreNames.MediaStore, () => {
	const defaultState: IMediaUrlStoreState = {
		mediaUrls: [],
	};

	const state = reactive<IMediaUrlStoreState>(cloneDeep(defaultState));
	const photoObjectUrls = new Set<string>();

	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: StoreNames.MediaStore, isSuccess: true }).pipe(take(1));
		},
		getMediaDataDetailById(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaDTO> {
			return plexMediaApi
				.getMediaDetailByIdEndpoint(mediaId, {
					type: mediaType,
				})
				.pipe(map((response) => response.value!));
		},
		getMediaComparisonDetails(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaComparisonDetailsDTO> {
			return plexMediaApi
				.getMediaComparisonDetailsEndpoint(mediaId, {
					type: mediaType,
				})
				.pipe(map((response) => response.value!));
		},
		getMediaThumbnailUrl(query: {
			plexServerId: number;
			plexKey: string;
			metaDataKey: number;
			height: number;
			width: number;
		}, requestOptions?: Pick<AxiosRequestConfig, 'handleErrorsLocally'>): Observable<string> {
			// Fast-path: return cached object URL if present
			const existing = state.mediaUrls.find((x) => x.plexServerId === query.plexServerId && x.plexKey === query.plexKey && x.metaDataKey === query.metaDataKey);
			if (existing)
				return of(existing.url);

			return from(
				Axios.request<Blob | BaseResultDTO>({
					url: `/api/PlexMedia/thumbnail`,
					method: 'GET',
					params: query,
					responseType: 'blob',
					...requestOptions,
				}),
			)
				.pipe(
					map((res) => {
						if (res.status === 200) {
							return actions.updateMediaUrl({
								plexServerId: query.plexServerId,
								plexKey: query.plexKey,
								metaDataKey: query.metaDataKey,
								image: res.data as Blob,
							});
						}
						Log.warn('Failed to get media thumbnail image', res);
						return '';
					}),
					catchError((error) => {
					// Handle network errors, timeouts, 502/504 gateway errors silently
					// Return empty string to trigger fallback image display
						Log.debug('Media thumbnail request failed', { query, error: error?.message || error });
						return of('');
					}),
				);
		},

		getPhotoImageUrl(asset: PhotoAssetBrowseDTO, representation: 'original' | 'converted'): Observable<string> {
			return plexPhotoApi.getPhotoAssetImageEndpoint(
				asset.plexLibraryId,
				asset.id,
				{ representation: representation === 'original' ? PhotoImageRepresentation.Original : PhotoImageRepresentation.Converted },
				{
					handleErrorsLocally: true,
					validateStatus: (status) => status !== 401,
					// Expected image failures belong to the viewer; keep their status beside raw blob successes.
					transformResponse: (data: Blob, _headers, status) => status === 200
						? data
						: { isSuccess: false, statusCode: status, value: null, errors: [], successes: [] },
				},
			).pipe(
				map((response) => {
					if (!(response instanceof Blob)) {
						if (response.statusCode === 404) {
							throw Object.assign(new Error('The photo asset is no longer available.'), { statusCode: 404 });
						}
						return '';
					}
					if (!response.type.startsWith('image/')) return '';
					const url = URL.createObjectURL(response);
					photoObjectUrls.add(url);
					return url;
				}),
			);
		},
		releasePhotoImageUrl(url: string): void {
			if (photoObjectUrls.delete(url)) URL.revokeObjectURL(url);
		},

		updateMediaUrl({
			plexServerId,
			plexKey,
			metaDataKey,
			image,
		}: {
			plexServerId: number;
			plexKey: string;
			metaDataKey: number;
			image: Blob;
		}): string {
			const index = state.mediaUrls.findIndex((x) => x.plexServerId === plexServerId && x.plexKey === plexKey && x.metaDataKey === metaDataKey);
			const mediaObject = Object.freeze({
				plexServerId,
				plexKey,
				metaDataKey,
				url: URL.createObjectURL(image),
			});

			void (index === -1 ? state.mediaUrls.push(mediaObject) : state.mediaUrls.splice(index, 1, mediaObject));

			return mediaObject.url;
		},
		$reset() {
			for (const url of photoObjectUrls) URL.revokeObjectURL(url);
			photoObjectUrls.clear();
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {};
	return {
		...toRefs(state), ...actions, ...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaStore, import.meta.hot));
}
