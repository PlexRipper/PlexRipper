import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { SettingsModel } from '@dto/mainApi';
import ResultDTO from '@dto/ResultDTO';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From SettingsAPI => ';
const apiPath = '/settings';

export function getSettings(): Observable<ResultDTO<SettingsModel>> {
	preApiRequest(logText, 'getSettings');
	const result = Axios.get<ResultDTO<SettingsModel>>(`${apiPath}`);
	return checkResponse<ResultDTO<SettingsModel>>(result, logText, 'getSettings');
}

export function updateSettings(settings: SettingsModel): Observable<ResultDTO<SettingsModel>> {
	preApiRequest(logText, 'updateSettings');
	const result = Axios.put<ResultDTO<SettingsModel>>(`${apiPath}`, settings);
	return checkResponse<ResultDTO<SettingsModel>>(result, logText, 'updateSettings');
}

export function resetDatabase(): Observable<ResultDTO> {
	preApiRequest(logText, 'resetDatabase');
	const result: Observable<AxiosResponse> = Axios.get<ResultDTO>(`${apiPath}/ResetDb`);
	return checkResponse<ResultDTO>(result, logText, 'resetDatabase');
}
