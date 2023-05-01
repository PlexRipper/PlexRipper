import { map, mergeMap, take } from 'rxjs/operators';
import { Observable, Observer, of, switchMap, throwError } from 'rxjs';
import Log from 'consola';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService, LibraryService } from '@service';
import { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { getLibraryMediaData, getThumbnail, getTvShow } from '@api/mediaApi';
import ISetupResult from '@interfaces/service/ISetupResult';

export class MediaService extends BaseService {
	public constructor() {
		super('MediaService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					mediaUrls: state.mediaUrls,
				};
			},
		});
	}

	setup(): Observable<ISetupResult> {
		super.setup();
		return of({ name: this._name, isSuccess: true }).pipe(take(1));
	}

	public getThumbnail(mediaId: number, mediaType: PlexMediaType, width = 0, height = 0): Observable<string> {
		return new Observable((observer: Observer<string>) => {
			const mediaUrls = this.getState().mediaUrls;
			const mediaUrl = mediaUrls.find((x) => x.type === mediaType && x.id === mediaId);
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
							map((response) => {
								if (response.data) {
									// Convert imageUrl to objectUrl
									const imageUrl: string = URL.createObjectURL(response.data);
									if (imageUrl) {
										this.updateStore('mediaUrls', { id: mediaId, type: mediaType, url: imageUrl });
									}
									return imageUrl;
								}
								return '';
							}),
					  ),
			),
			take(1),
		);
	}

	public getMediaData(plexLibraryId: number, page: number, size: number): Observable<PlexMediaSlimDTO[]> {
		return getLibraryMediaData(plexLibraryId, page, size).pipe(
			map((response) => {
				if (response && response.isSuccess) {
					return response.value ?? [];
				}
				return [];
			}),
		);
	}

	public getMediaDataById(mediaId: number, mediaType: PlexMediaType): Observable<PlexMediaDTO | null> {
		switch (mediaType) {
			case PlexMediaType.TvShow:
				return this.getTvShowMediaData(mediaId);
			default:
				return throwError(() => {
					return new Error(`MediaType with ${mediaType} is not supported in getMediaDataById`);
				});
		}
	}

	public getTvShowMediaData(mediaId: number): Observable<PlexMediaDTO | null> {
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
	}
}

export default new MediaService();
