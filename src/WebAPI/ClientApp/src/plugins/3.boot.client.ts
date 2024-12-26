import Log from 'consola';
import Axios from 'axios';
import { useGlobalStore, useLocalizationStore } from '@store';
import type IAppConfig from '@class/IAppConfig';
import type { Router } from 'vue-router';
import type { I18nObjectType } from '@interfaces';
import { defineNuxtPlugin } from '#app';

export default defineNuxtPlugin((nuxtApp) => {
	const publicEnv = useRuntimeConfig().public;

	nuxtApp.hook('app:created', () => {
		Log.level = 4;
		// Log.level = config.public.isProduction ? LogLevel.Debug : LogLevel.Debug;

		let baseUrl = `http://localhost:${publicEnv.apiPort}`;
		if (publicEnv.isDocker) {
			const currentLocation = window.location;
			baseUrl = `${currentLocation.protocol}//${currentLocation.hostname}:${currentLocation.port}`;
		}

		Log.info('nuxtApp:', nuxtApp);
		const appConfig: IAppConfig = {
			nodeEnv: publicEnv.nodeEnv,
			isProduction: publicEnv.nodeEnv === 'production',
			isDocker: publicEnv.isDocker,
			baseUrl,
		};
		setupAxios(appConfig, nuxtApp.$router as Router);
		useLocalizationStore().setI18nObject(nuxtApp.$i18n as I18nObjectType);
		useGlobalStore()
			.setupServices({ config: appConfig })
			.subscribe();
	});
});

function setupAxios(appConfig: IAppConfig, router: Router) {
	Axios.defaults.baseURL = appConfig.baseUrl;
	Axios.defaults.withCredentials = true;

	// Source: https://github.com/axios/axios/issues/41#issuecomment-484546457
	// Now error resolves in catch block rather than then block.
	//	Axios.defaults.validateStatus = () => true;

	// Source: https://github.com/axios/axios/issues/41#issuecomment-386762576
	Axios.interceptors.response.use(
		(config) => {
			useGlobalStore().setAppVersion(config.headers['x-plexripper-version']);
			return config;
		},
		(error) => {
			const status = error.response?.status;

			// Redirect to log-in on 401 Unauthorized
			if (status === 401) {
				router.push('/login'); // Redirect to the login page
				return Promise.reject('Unauthorized');
			}

			// Optionally, handle other error codes (e.g., 403 Forbidden)
			if (status === 403) {
				// Example: Redirect to access denied page or show a message
				router.push('/access-denied');
				return Promise.reject('Access Denied');
			}

			// Reject the promise to ensure the calling code can still handle the error
			return Promise.reject(error);
		},
	);
}
