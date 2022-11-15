import Vue from 'vue';
import Log from 'consola';
import { Context } from '@nuxt/types';
import { GlobalService } from '@service';

export default (ctx: Context): void => {
	GlobalService.getConfigReady().subscribe((config) => {
		// Setup logging
		Vue.config.devtools = true;
		Vue.config.productionTip = false;
		Log.level = config.isProduction ? 5 : 5;
	});

	// Setup Config
	Log.info(`Nuxt Environment: ${ctx.$config.nodeEnv}`);
	GlobalService.setConfigReady(ctx.$config);
	GlobalService.setup(ctx);
};
