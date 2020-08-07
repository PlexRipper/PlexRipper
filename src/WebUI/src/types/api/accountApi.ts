import { PlexAccountDTO } from '@dto/mainApi';
import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import Result from 'fluent-type-results';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<PlexAccountDTO[]> {
	preApiRequest(logText, 'getAllAccounts');
	const result = Axios.get<Result<PlexAccountDTO[]>>(apiPath);
	return checkResponse<PlexAccountDTO[]>(result, logText, 'getAllAccounts');
}

export function getAllEnabledAccounts(): Observable<PlexAccountDTO[]> {
	preApiRequest(logText, 'getAllEnabledAccounts');
	const result = Axios.get<Result<PlexAccountDTO[]>>(`${apiPath}/?enabledOnly=true`);
	return checkResponse<PlexAccountDTO[]>(result, logText, 'getAllEnabledAccounts');
}

export function validateAccount(account: PlexAccountDTO): Observable<boolean> {
	preApiRequest(logText, 'validateAccount');
	const result = Axios.post<Result<boolean>>(`${apiPath}/validate`, account);
	return checkResponse<boolean>(result, logText, 'validateAccount');
}

export function createAccount(account: PlexAccountDTO): Observable<PlexAccountDTO | null> {
	preApiRequest(logText, 'createAccount');
	const result = Axios.post<Result<PlexAccountDTO>>(apiPath, account);
	return checkResponse<PlexAccountDTO | null>(result, logText, 'createAccount');
}

export function updateAccount(account: PlexAccountDTO): Observable<PlexAccountDTO | null> {
	preApiRequest(logText, 'updateAccount');
	const result = Axios.put<Result<PlexAccountDTO>>(`${apiPath}/${account.id}`, account);
	return checkResponse<PlexAccountDTO | null>(result, logText, 'updateAccount');
}

export function deleteAccount(accountId: Number): Observable<boolean> {
	preApiRequest(logText, 'deleteAccount');
	const result = Axios.delete<Result<boolean>>(`${apiPath}/${accountId}`);
	return checkResponse<boolean>(result, logText, 'deleteAccount');
}

export function getAccount(accountId: Number): Observable<PlexAccountDTO> {
	preApiRequest(logText, 'getAccount');
	const result = Axios.get<Result<PlexAccountDTO>>(`${apiPath}/${accountId}`);
	return checkResponse<PlexAccountDTO>(result, logText, 'getAccount');
}
