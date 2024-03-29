import { acceptHMRUpdate, defineStore } from 'pinia';
import { Observable, of, switchMap, throwError, type Observer } from 'rxjs';
import { map, mergeMap, take } from 'rxjs/operators';
import type { ISetupResult, IObjectUrl } from '@interfaces';
import { PlexMediaType, type PlexMediaDTO, type PlexMediaSlimDTO } from '@dto/mainApi';
import { getLibraryMediaData, getThumbnail, getMediaDetailByIdEndpoint } from '@api/mediaApi';

export const useMediaStore = defineStore('MediaStore', () => {
	const state = reactive<{ mediaUrls: IObjectUrl[] }>({
		mediaUrls: [],
	});
	const actions = {
		setup(): Observable<ISetupResult> {
			return of({ name: useMediaStore.name, isSuccess: true }).pipe(take(1));
		},
		getThumbnail(mediaId: number, mediaType: PlexMediaType, width = 0, height = 0): Observable<string> {
			return new Observable((observer: Observer<string>) => {
				const mediaUrl = state.mediaUrls.find((x) => x.type === mediaType && x.id === mediaId);
				if (mediaUrl) {
					observer.next(mediaUrl.url);
				}
				observer.next('');
			}).pipe(
				// We use a mergeMap here as an if conditional, return the url if found and otherwise fetch
				mergeMap((value) =>
					value !== ''
						? of(value)
						: getThumbnail(mediaId, mediaType, width, height).pipe(
								switchMap((response) => {
									if (response.data) {
										// Convert imageUrl to objectUrl
										const imageUrl: string = URL.createObjectURL(response.data);
										if (imageUrl) {
											actions.updateMediaUrl({ id: mediaId, type: mediaType, url: imageUrl });
										}
										return of(imageUrl);
									}
									return throwError(() => {
										return new Error(`MediaType with ${mediaType} is not supported in getMediaDataById`);
									});
								}),
							),
				),
				take(1),
			);
		},
		getMediaData(plexLibraryId: number, page: number, size: number): Observable<PlexMediaSlimDTO[]> {
			return getLibraryMediaData(plexLibraryId, page, size).pipe(
				map((response) => {
					if (response && response.isSuccess) {
						return response.value ?? [];
					}
					return [];
				}),
			);
		},
		getMediaDataDetailById(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaDTO> {
			return getMediaDetailByIdEndpoint(mediaId, mediaType);
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
