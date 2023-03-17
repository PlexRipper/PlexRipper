import Vue from 'vue';
import Log, { LogLevel } from 'consola';

export default defineNuxtPlugin(() => {
	const config = useRuntimeConfig();

	// Set initial logging config to start logging a.s.a.p
	// Vue.config.devtools = true;
	// Vue.config.productionTip = false;

	Log.level = LogLevel.Debug;
	// Setup Config
	Log.info(`Nuxt Environment: ${config.public.version}`);
	// Log.level = config.public.isProduction ? LogLevel.Debug : LogLevel.Debug;
});
