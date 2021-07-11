import Vue from 'vue';
import Log from 'consola';
import { Context } from '@nuxt/types';
import { GlobalService } from '@state';

export default (ctx: Context): void => {
	GlobalService.getConfigReady().subscribe((config) => {
		// Setup logging
		Vue.config.devtools = !config.isProduction;
		Vue.config.productionTip = false;
		Log.level = config.isProduction ? 3 : 5;
	});

	// Setup Config
	Log.info(`Nuxt Environment: ${ctx.$config.nodeEnv}`);
	GlobalService.setConfigReady(ctx.$config);
	GlobalService.setup(ctx);
};
