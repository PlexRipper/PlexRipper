import Log from 'consola';
import { GlobalService } from '@service';
import IAppConfig from '@class/IAppConfig';
import Axios from 'axios-observable';

export default defineNuxtPlugin(() => {
	const config = useRuntimeConfig();

	Log.level = 4;
	// Log.level = config.public.isProduction ? LogLevel.Debug : LogLevel.Debug;
	Log.info(`Nuxt Environment: ${config.public.version}`);
	const publicEnv = useRuntimeConfig().public;

	const appConfig: IAppConfig = {
		version: publicEnv.version,
		nodeEnv: publicEnv.nodeEnv,
		isProduction: publicEnv.nodeEnv === 'production',
		baseUrl: `http://localhost:${publicEnv.apiPort}`,
	};

	setupAxios(appConfig);

	GlobalService.setupServices(appConfig);
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
