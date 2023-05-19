import Log from 'consola';
import { GlobalService } from '@service';
import IAppConfig from '@class/IAppConfig';

export default defineNuxtPlugin(() => {
	const config = useRuntimeConfig();

	Log.level = 4;
	// Log.level = config.public.isProduction ? LogLevel.Debug : LogLevel.Debug;
	Log.info(`Nuxt Environment: ${config.public.version}`);
	const publicConfig = useRuntimeConfig().public;

	const appConfig: IAppConfig = {
		version: publicConfig.version,
		nodeEnv: publicConfig.nodeEnv,
		isProduction: publicConfig.nodeEnv === 'production',
		baseURL: publicConfig.baseURL,
		baseApiUrl: new URL('/api', publicConfig.baseURL).toString(),
	};

	GlobalService.setupServices(appConfig);
});
