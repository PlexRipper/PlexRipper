import Log from 'consola';
import { SettingsService } from '@state';

let subscriptionCreated: boolean = false;
export default function ({ route, redirect }) {
	// Redirect to setup if it is the first time setup
	if (!subscriptionCreated) {
		subscriptionCreated = true;

		SettingsService.getFirstTimeSetup().subscribe((state) => {
			if (state === null) {
				return;
			}
			Log.info('Redirecting to the setup page');
			if (state && route.path !== '/setup') {
				return redirect('/setup');
			}
		});
	}
}
