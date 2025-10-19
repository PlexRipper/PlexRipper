import { acceptHMRUpdate, defineStore } from 'pinia';
import { from, type Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
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
			return from(
				Axios.request<Blob | BaseResultDTO>({
					url: `/api/PlexMedia/thumbnail`,
					method: 'GET',
					params: query,
					responseType: 'blob',
				}),
			).pipe(map((res) => {
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
			}));
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
			const url = URL.createObjectURL(image);
			const x = {
				plexServerId,
				plexKey,
				metaDataKey,
				url,
			};

			void (index === -1 ? state.mediaUrls.push(x) : state.mediaUrls.splice(index, 1, x));

			return url;
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
