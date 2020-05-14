import Log from 'consola';
import IAccount from '@dto/IAccount';
import { GlobalStore } from '@/store';
const logText = 'From AccountAPI => ';

export async function GetAllAccounts(): Promise<IAccount[]> {
	try {
		let accounts: IAccount[] = [];

		await GlobalStore.Axios.get('/accounts').then((x) => {
			accounts = x.data;
		});
		Log.debug(logText, accounts);
		return accounts;
	} catch (error) {
		Log.error(logText, error);
	}
	return [];
}
