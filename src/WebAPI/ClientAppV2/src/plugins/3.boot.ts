import Log from 'consola';
import { GlobalService } from '@service';

export default defineNuxtPlugin(() => {
	const config = useRuntimeConfig();

	Log.level = 4;
	// Log.level = config.public.isProduction ? LogLevel.Debug : LogLevel.Debug;
	Log.info(`Nuxt Environment: ${config.public.version}`);

	GlobalService.setupServices();
});
