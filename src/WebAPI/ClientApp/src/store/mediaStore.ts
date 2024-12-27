import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { PlexMediaType, PlexMediaDTO } from '@dto';
import type { ISetupResult, IObjectUrl } from '@interfaces';
import { plexMediaApi } from '@api';
import { cloneDeep } from 'lodash-es';

interface IMediaUrlStoreState {
	mediaUrls: IObjectUrl[];
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
		updateMediaUrl(mediaUrl: IObjectUrl) {
			const index = state.mediaUrls.findIndex((x) => x.type === mediaUrl.type && x.id === mediaUrl.id);
			if (index === -1) {
				state.mediaUrls.push(mediaUrl);
				return;
			}

			state.mediaUrls.splice(index, 1, mediaUrl);
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};
	const getters = {};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaStore, import.meta.hot));
}
