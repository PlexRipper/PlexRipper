import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { PlexMediaType, PlexMediaDTO, PlexMediaSlimDTO } from '@dto';
import type { ISetupResult, IObjectUrl } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';

export const useMediaStore = defineStore('MediaStore', () => {
	const state = reactive<{ mediaUrls: IObjectUrl[] }>({
		mediaUrls: [],
	});
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: useMediaStore.name, isSuccess: true }).pipe(take(1));
		},
		getMediaData(plexLibraryId: number, page: number, size: number): Observable<PlexMediaSlimDTO[]> {
			return plexLibraryApi
				.getPlexLibraryMediaEndpoint(plexLibraryId, {
					page,
					size,
				})
				.pipe(
					map((response) => {
						if (response && response.isSuccess) {
							return response.value ?? [];
						}
						return [];
					}),
				);
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
