import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { PlexAccountDTO, SettingsModel } from '@dto/mainApi';
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
	const result: Observable<AxiosResponse> = Axios.put<Result<boolean>>(`${apiPath}`, settings);
	return checkResponse<SettingsModel>(result, logText, 'updateSettings');
}

export function getActiveAccount(): Observable<PlexAccountDTO | null> {
	preApiRequest(logText, 'getActiveAccount');
	const result: Observable<AxiosResponse> = Axios.get<Result<PlexAccountDTO>>(`${apiPath}/activeaccount`);
	return checkResponse<PlexAccountDTO | null>(result, logText, 'getActiveAccount');
}

export function setActiveAccount(accountId: number): Observable<PlexAccountDTO | null> {
	preApiRequest(logText, 'setActiveAccount');
	const result: Observable<AxiosResponse> = Axios.put<Result<PlexAccountDTO>>(`${apiPath}/activeaccount/${accountId}`);
	return checkResponse<PlexAccountDTO | null>(result, logText, 'setActiveAccount');
}
