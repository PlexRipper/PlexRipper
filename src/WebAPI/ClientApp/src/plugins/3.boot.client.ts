import Log from 'consola';
import Axios from 'axios-observable';
import type IAppConfig from '@class/IAppConfig';
import type { I18nObjectType } from '@interfaces';
import { useGlobalStore } from '#imports';

export default defineNuxtPlugin((nuxtApp) => {
	const publicEnv = useRuntimeConfig().public;

	nuxtApp.hook('app:created', () => {
		Log.level = 4;
		// Log.level = config.public.isProduction ? LogLevel.Debug : LogLevel.Debug;
		Log.info(`Nuxt Environment: ${publicEnv.version}`);

		let baseUrl = `http://localhost:${publicEnv.apiPort}`;
		if (publicEnv.isDocker) {
			const currentLocation = window.location;
			baseUrl = `${currentLocation.protocol}//${currentLocation.hostname}:${currentLocation.port}`;
		}

		const appConfig: IAppConfig = {
			version: publicEnv.version,
			nodeEnv: publicEnv.nodeEnv,
			isProduction: publicEnv.nodeEnv === 'production',
			isDocker: publicEnv.isDocker,
			baseUrl,
		};
		setupAxios(appConfig);
		useGlobalStore()
			.setupServices({ config: appConfig, i18n: nuxtApp.$i18n as I18nObjectType })
			.subscribe();
	});
});

function setupAxios(appConfig: IAppConfig) {
	Axios.defaults.baseURL = appConfig.baseUrl + '/api';

	// Source: https://github.com/axios/axios/issues/41#issuecomment-484546457
	Axios.defaults.validateStatus = () => true;
	// Now error resolves in catch block rather than then block.
	// Source: https://github.com/axios/axios/issues/41#issuecomment-386762576
	Axios.interceptors.response.use(
		(config) => {
			return config;
		},
		(error) => {
			// Prevents 400 & 500 status code throwing exceptions
			return Promise.reject(error);
		},
	);
}
