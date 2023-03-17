import { map, mergeMap, take } from 'rxjs/operators';

import { Observable, Observer, of } from 'rxjs';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService } from '@service';
import { PlexMediaType } from '@dto/mainApi';
import { getThumbnail } from '@api/mediaApi';
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
}

const mediaService = new MediaService();
export default mediaService;
