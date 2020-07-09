import Log from 'consola';
import addSignalR from './signalrPlugin';
import { UserStore } from '@/store/';

export default async (): Promise<void> => {
	Log.info('Startup initialized');

	await UserStore.refreshAccounts();

	await addSignalR();
};
