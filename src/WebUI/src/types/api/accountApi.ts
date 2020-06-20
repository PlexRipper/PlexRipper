import Log from 'consola';
import IAccount from '@dto/IAccount';
import { GlobalStore } from '@/store';
const logText = 'From AccountAPI => ';
const apiPath = '/account';

export async function getAllAccountsAsync(): Promise<IAccount[]> {
	try {
		let accounts: IAccount[] = [];

		await GlobalStore.Axios.get(apiPath).then((x) => {
			accounts = x.data;
		});
		Log.debug(logText, accounts);
		return accounts;
	} catch (error) {
		Log.error(logText, error);
	}
	return [];
}

export async function ValidateAccountAsync(account: IAccount): Promise<Number> {
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

export async function createAccountAsync(account: IAccount): Promise<Number> {
	return await GlobalStore.Axios.post(apiPath, account)
		.then((res) => {
			Log.debug(logText + 'The validation api result: ', res);
			return res.status;
		})
		.catch((e) => {
			Log.error(logText + 'Validation Api Error: ', e);
			return e.response.status;
		});
}

export async function updateAccountAsync(account: IAccount): Promise<Number> {
	return await GlobalStore.Axios.put(`${apiPath}/${account.id}`, account)
		.then((res) => {
			Log.debug(logText + 'The validation api result: ', res);
			return res.status;
		})
		.catch((e) => {
			Log.error(logText + 'Validation Api Error: ', e);
			return e.response.status;
		});
}

export async function deleteAccountAsync(accountId: Number): Promise<boolean> {
	if (accountId > 0) {
		await GlobalStore.Axios.delete(`${apiPath}/${accountId}`)
			.then((res) => {
				Log.debug(logText + 'The validation api result: ', res);
				return res.status;
			})
			.catch((e) => {
				Log.error(logText + 'Validation Api Error: ', e);
				return e.response.status;
			});
	}
	Log.error(logText + 'Could not delete Account as the ID is 0');
	return false;
}
