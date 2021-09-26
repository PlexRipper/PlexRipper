import Log from 'consola';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService } from '@service';
import { PlexMediaType } from '@dto/mainApi';
import { Observable, of } from 'rxjs';
import { getThumbnail } from '@api/mediaApi';
import { map } from 'rxjs/operators';

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
