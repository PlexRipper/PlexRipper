import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';

const logText = 'From plexMediaApi => ';
const apiPath = '/PlexMedia';

export function getTvShow(id: number): Observable<PlexMediaDTO> {
	preApiRequest(logText, 'getTvShow');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/tvshow/${id}`);
	return checkResponse<PlexMediaDTO>(result, logText, 'getTvShow');
}

export function getThumbnail(
	plexMediaId: number,
	plexMediaType: PlexMediaType,
	width: number = 0,
	height: number = 0,
): Observable<AxiosResponse> {
	preApiRequest(logText, 'getThumbnail');
	return Axios.get(`${apiPath}/thumb?PlexMediaId=${plexMediaId}&PlexMediaType=${plexMediaType}&Width=${width}&Height=${height}`, {
		responseType: 'blob',
	});
}
