import { map, take } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import { EMPTY, Observable, of } from 'rxjs';
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

	setup(nuxtContext: Context): Observable<ISetupResult> {
		super.setup(nuxtContext);
		return of({ name: this._name, isSuccess: true }).pipe(take(1));
	}

	public getThumbnail(mediaId: number, mediaType: PlexMediaType, width: number = 0, height: number = 0): Observable<string> {
		const mediaUrls = this.getState().mediaUrls;

		const mediaUrl = mediaUrls.find((x) => x.type === mediaType && x.id === mediaId);
		if (mediaUrl) {
			return of(mediaUrl.url);
		}

		return getThumbnail(mediaId, mediaType, width, height).pipe(
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
		);
	}
}

const mediaService = new MediaService();
export default mediaService;
