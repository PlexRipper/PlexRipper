import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { Observable } from 'rxjs';
import { PlexTvShowDTO } from '@dto/mainApi';
import { checkResponse, preApiRequest } from '@api/baseApi';

const logText = 'From folderPathApi => ';
const apiPath = '/PlexMedia';

export function getTvShow(id: number): Observable<PlexTvShowDTO> {
	preApiRequest(logText, 'getTvShow');
	const result: Observable<AxiosResponse> = Axios.get(`${apiPath}/tvshow/${id}`);
	return checkResponse<PlexTvShowDTO>(result, logText, 'getTvShow');
}
