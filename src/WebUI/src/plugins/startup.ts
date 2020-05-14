import Log from 'consola';
import { UserStore } from '@/store/';

export default async (): Promise<void> => {
	Log.info('Startup initialized');

	await UserStore.refreshAccounts();
};
