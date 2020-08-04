import IPlexAccount from '@dto/IPlexAccount';
import { Observable } from 'rxjs';
import Axios from 'axios-observable';
import Result from 'fluent-type-results';
import { checkResponse } from './baseApi';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<IPlexAccount[] | null> {
	const result = Axios.get<Result<IPlexAccount[]>>(apiPath);
	return checkResponse<IPlexAccount[] | null>(result, logText, 'getAllAccounts');
}

export function getAllEnabledAccounts(): Observable<IPlexAccount[] | null> {
	const result = Axios.get<Result<IPlexAccount[]>>(`${apiPath}/?enabledOnly=true`);
	return checkResponse<IPlexAccount[] | null>(result, logText, 'getAllEnabledAccounts');
}

export function validateAccount(account: IPlexAccount): Observable<boolean> {
	const result = Axios.post<Result<boolean>>(`${apiPath}/validate`, account);
	return checkResponse<boolean>(result, logText, 'validateAccount');
}

export function createAccount(account: IPlexAccount): Observable<IPlexAccount | null> {
	const result = Axios.post<Result<IPlexAccount>>(apiPath, account);
	return checkResponse<IPlexAccount | null>(result, logText, 'createAccount');
}

export function updateAccount(account: IPlexAccount): Observable<IPlexAccount | null> {
	const result = Axios.put<Result<IPlexAccount>>(`${apiPath}/${account.id}`, account);
	return checkResponse<IPlexAccount | null>(result, logText, 'updateAccount');
}

export function deleteAccount(accountId: Number): Observable<boolean> {
	const result = Axios.delete<Result<boolean>>(`${apiPath}/${accountId}`);
	return checkResponse<boolean>(result, logText, 'deleteAccount');
}

export function getAccount(accountId: Number): Observable<IPlexAccount> {
	const result = Axios.get<Result<IPlexAccount>>(`${apiPath}/${accountId}`);
	return checkResponse<IPlexAccount>(result, logText, 'getAccount');
}
