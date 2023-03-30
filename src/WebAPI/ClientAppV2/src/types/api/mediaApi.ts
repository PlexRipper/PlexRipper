import { Observable } from 'rxjs';
import { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';

export function getTvShow(id: number): Observable<ResultDTO<PlexMediaDTO>> {
	return PlexRipperAxios.get<PlexMediaDTO>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/tvshow/${id}`,
		apiCategory: '',
		apiName: getTvShow.name,
	});
}

export function getLibraryMediaData(id: number, page: number, size: number): Observable<ResultDTO<PlexMediaSlimDTO[]>> {
	return PlexRipperAxios.get<PlexMediaSlimDTO[]>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/library/${id}`,
		params: { page, size },
		apiCategory: '',
		apiName: getLibraryMediaData.name,
	});
}

export function getThumbnail(plexMediaId: number, plexMediaType: PlexMediaType, width = 0, height = 0): Observable<any> {
	return PlexRipperAxios.getImage<Blob>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/thumb`,
		params: {
			plexMediaId,
			plexMediaType,
			width,
			height,
		},
		responseType: 'blob',
	});
}
