import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { preApiRequest } from '@api/baseApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';

const logText = 'From plexMediaApi => ';

export function getTvShow(id: number): Observable<ResultDTO<PlexMediaDTO>> {
	return PlexRipperAxios.get<PlexMediaDTO>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/tvshow/${id}`,
		apiCategory: logText,
		apiName: getTvShow.name,
	});
}

export function getThumbnail(
	plexMediaId: number,
	plexMediaType: PlexMediaType,
	width: number = 0,
	height: number = 0,
): Observable<any> {
	preApiRequest(logText, 'getThumbnail');
	return Axios.get<Blob>(`${PLEX_MEDIA_RELATIVE_PATH}/thumb`, {
		params: {
			plexMediaId,
			plexMediaType,
			width,
			height,
		},
		responseType: 'blob',
	});
}
