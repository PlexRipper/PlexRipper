import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs } from 'vue';
import { from, type Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, take, catchError } from 'rxjs/operators';
import type { PlexMediaType, PlexMediaDTO, BaseResultDTO } from '@dto';
import type { ISetupResult } from '@interfaces';
import { plexMediaApi } from '@api';
import { cloneDeep } from 'lodash-es';
import Log from 'consola';
import Axios from 'axios';

interface IMediaUrlStoreState {
	mediaUrls: IObjectUrl[];
}

interface IObjectUrl {
	plexServerId: number;
	plexKey: string;
	metaDataKey: number;
	url: string;
}

export const useMediaStore = defineStore('MediaStore', () => {
	const defaultState: IMediaUrlStoreState = {
		mediaUrls: [],
	};

	const state = reactive<IMediaUrlStoreState>(cloneDeep(defaultState));

	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: 'useMediaStore', isSuccess: true }).pipe(take(1));
		},
		getMediaDataDetailById(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaDTO> {
			return plexMediaApi
				.getMediaDetailByIdEndpoint(mediaId, {
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
		}): Observable<string> {
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
