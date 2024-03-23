import { Observable } from 'rxjs';
import type { PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import type ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';
import { PLEX_LIBRARY_RELATIVE_PATH, PLEX_MEDIA_RELATIVE_PATH } from '@api-urls';

export function getTvShow(id: number): Observable<ResultDTO<PlexMediaSlimDTO>> {
	return PlexRipperAxios.get<PlexMediaSlimDTO>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/tvshow/${id}`,
		apiCategory: '',
		apiName: getTvShow.name,
	});
}

export function getTvShowDetail(id: number): Observable<ResultDTO<PlexMediaDTO>> {
	return PlexRipperAxios.get<PlexMediaDTO>({
		url: `${PLEX_MEDIA_RELATIVE_PATH}/tvshow/detail/${id}`,
		apiCategory: '',
		apiName: getTvShowDetail.name,
	});
}

export function getLibraryMediaData(id: number, page: number, size: number): Observable<ResultDTO<PlexMediaSlimDTO[]>> {
	return PlexRipperAxios.get<PlexMediaSlimDTO[]>({
		url: `${PLEX_LIBRARY_RELATIVE_PATH}/${id}/media`,
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
