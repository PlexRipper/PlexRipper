import Log from 'consola';
import IPlexAccount from '@dto/IPlexAccount';
import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map, tap } from 'rxjs/operators';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<IPlexAccount[] | null> {
	const result: Observable<AxiosResponse> = Axios.get<IPlexAccount[]>(apiPath);
	return result.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(logText + 'getAllEnabledAccounts response:', data)),
	);
}

export function getAllEnabledAccounts(): Observable<IPlexAccount[] | null> {
	const result: Observable<AxiosResponse> = Axios.get<IPlexAccount[]>(`${apiPath}/?enabledOnly=true`);
	return result.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(logText + 'getAllEnabledAccounts response:', data)),
	);
}

export function validateAccount(account: IPlexAccount): Observable<boolean> {
	const result: Observable<AxiosResponse> = Axios.post<boolean>(`${apiPath}/validate`, account);
	return result.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(logText + 'validateAccount response:', data)),
	);
}

export function createAccount(account: IPlexAccount): Observable<IPlexAccount> {
	const result: Observable<AxiosResponse> = Axios.post<IPlexAccount>(apiPath, account);
	return result.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(logText + 'createAccount response:', data)),
	);
}

export function updateAccount(account: IPlexAccount): Observable<IPlexAccount> {
	const result: Observable<AxiosResponse> = Axios.put<IPlexAccount>(`${apiPath}/${account.id}`, account);
	return result.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(logText + 'updateAccount response:', data)),
	);
}

export function deleteAccount(accountId: Number): Observable<IPlexAccount> {
	const result: Observable<AxiosResponse> = Axios.delete(`${apiPath}/${accountId}`);
	return result.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(logText + 'deleteAccount response:', data)),
	);
}
