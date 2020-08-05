import IPlexAccount from '@dto/IPlexAccount';
import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import Result from 'fluent-type-results';
import { checkResponse, preApiRequest } from './baseApi';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<IPlexAccount[]> {
	preApiRequest(logText, 'getAllAccounts');
	const result = Axios.get<Result<IPlexAccount[]>>(apiPath);
	return checkResponse<IPlexAccount[]>(result, logText, 'getAllAccounts');
}

export function getAllEnabledAccounts(): Observable<IPlexAccount[]> {
	preApiRequest(logText, 'getAllEnabledAccounts');
	const result = Axios.get<Result<IPlexAccount[]>>(`${apiPath}/?enabledOnly=true`);
	return checkResponse<IPlexAccount[]>(result, logText, 'getAllEnabledAccounts');
}

export function validateAccount(account: IPlexAccount): Observable<boolean> {
	preApiRequest(logText, 'validateAccount');
	const result = Axios.post<Result<boolean>>(`${apiPath}/validate`, account);
	return checkResponse<boolean>(result, logText, 'validateAccount');
}

export function createAccount(account: IPlexAccount): Observable<IPlexAccount | null> {
	preApiRequest(logText, 'createAccount');
	const result = Axios.post<Result<IPlexAccount>>(apiPath, account);
	return checkResponse<IPlexAccount | null>(result, logText, 'createAccount');
}

export function updateAccount(account: IPlexAccount): Observable<IPlexAccount | null> {
	preApiRequest(logText, 'updateAccount');
	const result = Axios.put<Result<IPlexAccount>>(`${apiPath}/${account.id}`, account);
	return checkResponse<IPlexAccount | null>(result, logText, 'updateAccount');
}

export function deleteAccount(accountId: Number): Observable<boolean> {
	preApiRequest(logText, 'deleteAccount');
	const result = Axios.delete<Result<boolean>>(`${apiPath}/${accountId}`);
	return checkResponse<boolean>(result, logText, 'deleteAccount');
}

export function getAccount(accountId: Number): Observable<IPlexAccount> {
	preApiRequest(logText, 'getAccount');
	const result = Axios.get<Result<IPlexAccount>>(`${apiPath}/${accountId}`);
	return checkResponse<IPlexAccount>(result, logText, 'getAccount');
}
