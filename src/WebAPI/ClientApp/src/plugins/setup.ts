import Vue from 'vue';
import Log from 'consola';
import { Context } from '@nuxt/types';
import globalService from '@state/globalService';

export default (ctx: Context): void => {
	// Setup Config
	Log.info(`Nuxt Environment: ${ctx.$config.nodeEnv}`);
	globalService.setConfigReady(ctx.$config);

	// Setup logging
	const isProduction = ctx.$config.nodeEnv === 'production';

	Vue.config.devtools = !isProduction;
	Vue.config.productionTip = false;
	Log.level = isProduction ? 3 : 5;
};
