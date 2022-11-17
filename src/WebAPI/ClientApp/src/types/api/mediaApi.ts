import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { PlexMediaDTO, PlexMediaType, ServerDownloadProgressDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';
import ResultDTO from '@dto/ResultDTO';
import PlexRipperAxios from '@class/PlexRipperAxios';

const logText = 'From plexMediaApi => ';
const apiPath = '/PlexMedia';

export function getTvShow(id: number): Observable<ResultDTO<PlexMediaDTO>> {
	return PlexRipperAxios.get<PlexMediaDTO>({
		url: `${apiPath}/tvshow/${id}`,
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
	return Axios.get<Blob>(`${apiPath}/thumb`, {
		params: {
			plexMediaId,
			plexMediaType,
			width,
			height,
		},
		responseType: 'blob',
	});
}
