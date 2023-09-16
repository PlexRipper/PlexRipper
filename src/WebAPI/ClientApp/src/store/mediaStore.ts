import { acceptHMRUpdate, defineStore } from 'pinia';
import { Observable, Observer, of, switchMap, throwError } from 'rxjs';
import { map, mergeMap, take } from 'rxjs/operators';
import Log from 'consola';
import ISetupResult from '@interfaces/service/ISetupResult';
import IObjectUrl from '@interfaces/IObjectUrl';
import { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { getLibraryMediaData, getThumbnail, getTvShow, getTvShowDetail } from '@api/mediaApi';

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
		getMediaDataById(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaSlimDTO | null> {
			switch (mediaType) {
				case PlexMediaType.TvShow:
					return actions.getTvShowMediaData(mediaId);
				default:
					return throwError(() => {
						return new Error(`MediaType with ${mediaType} is not supported in getMediaDataById`);
					});
			}
		},
		getTvShowMediaData(mediaId: number): Observable<PlexMediaSlimDTO | null> {
			return getTvShow(mediaId).pipe(
				switchMap((response) => {
					if (response.isSuccess) {
						return of(response.value ?? null);
					}

					return throwError(() => {
						const error = new Error(`TvShow with id ${mediaId} was not found`);
						response.errors?.forEach((err) => {
							Log.error(err);
						});
						return error;
					});
				}),
			);
		},
		getMediaDataDetailById(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaDTO | null> {
			switch (mediaType) {
				case PlexMediaType.TvShow:
					return actions.getTvShowMediaDataDetail(mediaId);
				default:
					return throwError(() => {
						return new Error(`MediaType with ${mediaType} is not supported in getMediaDataDetailById`);
					});
			}
		},
		getTvShowMediaDataDetail(mediaId: number): Observable<PlexMediaDTO | null> {
			return getTvShowDetail(mediaId).pipe(
				switchMap((response) => {
					if (response.isSuccess) {
						return of(response.value ?? null);
					}

					return throwError(() => {
						const error = new Error(`TvShow with id ${mediaId} was not found`);
						response.errors?.forEach((err) => {
							Log.error(err);
						});
						return error;
					});
				}),
			);
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
