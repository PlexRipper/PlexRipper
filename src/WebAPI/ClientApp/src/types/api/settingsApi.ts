import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { checkResponse, preApiRequest } from './baseApi';
import { SettingsModelDTO } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';

const logText = 'From SettingsAPI => ';
const apiPath = '/settings';

export function getSettings(): Observable<ResultDTO<SettingsModelDTO>> {
	preApiRequest(logText, 'getSettings');
	const result = Axios.get<ResultDTO<SettingsModelDTO>>(`${apiPath}`);
	return checkResponse<ResultDTO<SettingsModelDTO>>(result, logText, 'getSettings');
}

export function updateSettings(settings: SettingsModelDTO): Observable<ResultDTO<SettingsModelDTO>> {
	preApiRequest(logText, 'updateSettings', settings);
	const result = Axios.put<ResultDTO<SettingsModelDTO>>(`${apiPath}`, settings);
	return checkResponse<ResultDTO<SettingsModelDTO>>(result, logText, 'updateSettings');
}

export function resetDatabase(): Observable<ResultDTO> {
	preApiRequest(logText, 'resetDatabase');
	const result: Observable<AxiosResponse> = Axios.get<ResultDTO>(`${apiPath}/ResetDb`);
	return checkResponse<ResultDTO>(result, logText, 'resetDatabase');
}
