import { PlexAccountDTO } from '@dto/mainApi';
import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import ResultDTO from '@dto/ResultDTO';
import { checkResponse, preApiRequest } from './baseApi';

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

export function validateAccount(account: PlexAccountDTO): Observable<ResultDTO<boolean>> {
	preApiRequest(logText, 'validateAccount');
	const result = Axios.post<ResultDTO<boolean>>(`${apiPath}/validate`, account);
	return checkResponse<ResultDTO<boolean>>(result, logText, 'validateAccount');
}

export function createAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
	preApiRequest(logText, 'createAccount');
	const result = Axios.post<ResultDTO<PlexAccountDTO>>(apiPath, account);
	return checkResponse<ResultDTO<PlexAccountDTO | null>>(result, logText, 'createAccount');
}

export function updateAccount(account: PlexAccountDTO): Observable<ResultDTO<PlexAccountDTO | null>> {
	preApiRequest(logText, 'updateAccount');
	const result = Axios.put<ResultDTO<PlexAccountDTO>>(`${apiPath}/${account.id}`, account);
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
