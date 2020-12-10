import Log from 'consola';
import SettingsService from '@service/settingsService';
import { settingsStore } from '~/store';

export default function ({ route, redirect }) {
	// Redirect to setup if it is the first time setup
	let subscriptionCreated: boolean = false;
	if (!subscriptionCreated) {
		subscriptionCreated = true;
		SettingsService.getSettings().subscribe(() => {
			Log.info('Redirecting to the setup page');
			if (settingsStore.firstTimeSetup && route.path !== '/setup') {
				return redirect('/setup');
			}
		});
	}
}
