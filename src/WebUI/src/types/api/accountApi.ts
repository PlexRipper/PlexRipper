import Log from 'consola';
import IPlexAccount from '@dto/IPlexAccount';
import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map } from 'rxjs/operators';
import { GlobalStore } from '@/store';

const logText = 'From AccountAPI => ';
const apiPath = '/plexaccount';

export function getAllAccounts(): Observable<IPlexAccount[] | null> {
	const result: Observable<AxiosResponse> = Axios.get<IPlexAccount[]>(apiPath);
	return result.pipe(map((res: AxiosResponse) => res.data));
}

export function getAllEnabledAccounts(): Observable<IPlexAccount[] | null> {
	const result: Observable<AxiosResponse> = Axios.get<IPlexAccount[]>(`${apiPath}/?enabledOnly=true`);
	return result.pipe(map((res: AxiosResponse) => res.data));
}

export async function ValidateAccountAsync(account: IPlexAccount): Promise<Number> {
	return await GlobalStore.Axios.post(apiPath + '/validate', account)
		.then((res) => {
			Log.debug(logText + 'The validation api result: ', res);
			return res.status;
		})
		.catch((e) => {
			Log.error(logText + 'Validation Api Error: ', e);
			return e.response.status;
		});
}

export async function createAccountAsync(account: IPlexAccount): Promise<Number> {
	return await GlobalStore.Axios.post(apiPath, account)
		.then((res) => {
			Log.debug(logText + 'createAccountAsync result: ', res);
			return res.status;
		})
		.catch((e) => {
			Log.error(logText + 'createAccountAsync Error: ', e);
			return e.response.status;
		});
}

export async function updateAccountAsync(account: IPlexAccount): Promise<Number> {
	return await GlobalStore.Axios.put(`${apiPath}/${account.id}`, account)
		.then((res) => {
			Log.debug(logText + 'updateAccountAsync result: ', res);
			return res.status;
		})
		.catch((e) => {
			Log.error(logText + 'updateAccountAsync Error: ', e);
			return e.response.status;
		});
}

export async function deleteAccountAsync(accountId: Number): Promise<boolean> {
	if (accountId > 0) {
		await GlobalStore.Axios.delete(`${apiPath}/${accountId}`)
			.then((res) => {
				Log.debug(logText + 'deleteAccountAsync result: ', res);
				return res.status;
			})
			.catch((e) => {
				Log.error(logText + 'deleteAccountAsync Error: ', e);
				return e.response.status;
			});
	}
	Log.error(logText + 'Could not delete Account as the ID is 0');
	return false;
}
