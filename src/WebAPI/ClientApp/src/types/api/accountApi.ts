import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import { checkResponse, preApiRequest } from './baseApi';
import ResultDTO from '@dto/ResultDTO';
import { AuthPin, PlexAccountDTO } from '@dto/mainApi';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<ResultDTO<PlexAccountDTO[]>> {
	preApiRequest(logText, 'getAllAccounts');
	const result = Axios.get<ResultDTO<PlexAccountDTO[]>>(apiPath);
	return checkResponse<ResultDTO<PlexAccountDTO[]>>(result, logText, 'getAllAccounts');
}

export function getAllEnabledAccounts(): Observable<ResultDTO<PlexAccountDTO[]>> {
	preApiRequest(logText, 'getAllEnabledAccounts');
	const result = Axios.get<ResultDTO<PlexAccountDTO[]>>(`${apiPath}/?enabledOnly=true`);
	return checkResponse<ResultDTO<PlexAccountDTO[]>>(result, logText, 'getAllEnabledAccounts');
}

export function validateAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO>> {
	preApiRequest(logText, 'validateAccount');
	const result = Axios.post<ResultDTO<PlexAccountDTO>>(`${apiPath}/validate`, account);
	return checkResponse<ResultDTO<PlexAccountDTO>>(result, logText, 'validateAccount');
}

export function createAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
	preApiRequest(logText, 'createAccount');
	const result = Axios.post<ResultDTO<PlexAccountDTO>>(apiPath, account);
	return checkResponse<ResultDTO<PlexAccountDTO | null>>(result, logText, 'createAccount');
}

export function updateAccount(account: PlexAccountDTO, inspect: boolean = false): Observable<ResultDTO<PlexAccountDTO | null>> {
	preApiRequest(logText, 'updateAccount');
	const result = Axios.put<ResultDTO<PlexAccountDTO>>(`${apiPath}/${account.id}?inspect=${inspect}`, account);
	return checkResponse<ResultDTO<PlexAccountDTO | null>>(result, logText, 'updateAccount');
}

export function deleteAccount(accountId: Number): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'deleteAccount');
	const result = Axios.delete<ResultDTO<boolean>>(`${apiPath}/${accountId}`);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'deleteAccount');
}

export function getAccount(accountId: Number): Observable<ResultDTO<PlexAccountDTO>> {
	preApiRequest(logText, 'getAccount');
	const result = Axios.get<ResultDTO<PlexAccountDTO>>(`${apiPath}/${accountId}`);
	return checkResponse<ResultDTO<PlexAccountDTO>>(result, logText, 'getAccount');
}

export function refreshAccount(accountId: Number): Observable<ResultDTO> {
	preApiRequest(logText, 'refreshAccount');
	const result = Axios.get<ResultDTO>(`${apiPath}/refresh/${accountId}`);
	return checkResponse<ResultDTO>(result, logText, 'refreshAccount');
}

export function GetAndCheck2FaPin(clientId: String, authPinId: number = 0): Observable<ResultDTO<AuthPin>> {
	preApiRequest(logText, 'getAuthPin');
	const result = Axios.get<ResultDTO<AuthPin>>(`${apiPath}/authpin`, {
		params: {
			clientId,
			authPinId,
		},
	});
	return checkResponse<ResultDTO<AuthPin>>(result, logText, 'getAuthPin');
}

export function checkAuthPin(clientId: String): Observable<ResultDTO<AuthPin>> {
	preApiRequest(logText, 'checkAuthPin');
	const result = Axios.get<ResultDTO<AuthPin>>(`${apiPath}/authpin/${clientId}/check`);
	return checkResponse<ResultDTO<AuthPin>>(result, logText, 'checkAuthPin');
}

export function generateClientId(): Observable<ResultDTO<string>> {
	preApiRequest(logText, 'generateClientId');
	const result = Axios.get<ResultDTO<string>>(`${apiPath}/clientid`);
	return checkResponse<ResultDTO<string>>(result, logText, 'generateClientId');
}
