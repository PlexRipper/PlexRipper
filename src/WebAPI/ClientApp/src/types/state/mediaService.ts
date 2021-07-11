import IStoreState from '@interfaces/IStoreState';
import { BaseService } from '@state';
import { PlexMediaType } from '@dto/mainApi';
import { Observable, of } from 'rxjs';
import { getThumbnail } from '@api/mediaApi';
import { switchMap } from 'rxjs/operators';

export class MediaService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					mediaUrls: state.mediaUrls,
				};
			},
		});
	}

	public getThumbnail(mediaId: number, mediaType: PlexMediaType, width: number = 0, height: number = 0): Observable<string> {
		const mediaUrls = this.getState().mediaUrls;

		const mediaUrl = mediaUrls.find((x) => x.type === mediaType && x.id === mediaId);
		if (mediaUrl) {
			return of(mediaUrl.url);
		}

		return getThumbnail(mediaId, mediaType, width, height).pipe(
			switchMap((response) => {
				if (response) {
					// Convert imageUrl to objectUrl
					const imageUrl: string = URL.createObjectURL(response.data);
					if (imageUrl) {
						// Update the mediaUrls
						mediaUrls.push({ id: mediaId, type: mediaType, url: imageUrl });
					}
					return of(imageUrl);
				}
				return of('');
			}),
		);
	}
}

const mediaService = new MediaService();
export default mediaService;
