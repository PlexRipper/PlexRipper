import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { ResultDTO, SettingsModel } from '@dto/mainApi';
import Result from 'fluent-type-results';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From SettingsAPI => ';
const apiPath = '/settings';

export function getSettings(): Observable<SettingsModel> {
	preApiRequest(logText, 'getSettings');
	const result: Observable<AxiosResponse> = Axios.get<Result<SettingsModel>>(`${apiPath}`);
	return checkResponse<SettingsModel>(result, logText, 'getSettings');
}

export function updateSettings(settings: SettingsModel): Observable<SettingsModel> {
	preApiRequest(logText, 'updateSettings');
	const result: Observable<AxiosResponse> = Axios.put<Result<SettingsModel>>(`${apiPath}`, settings);
	return checkResponse<SettingsModel>(result, logText, 'updateSettings');
}

export function resetDatabase(): Observable<ResultDTO> {
	preApiRequest(logText, 'resetDatabase');
	const result: Observable<AxiosResponse> = Axios.get<Result>(`${apiPath}/ResetDb`);
	return checkResponse<ResultDTO>(result, logText, 'resetDatabase');
}
