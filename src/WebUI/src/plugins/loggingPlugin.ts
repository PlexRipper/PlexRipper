import Vue from 'vue';
import { Context } from '@nuxt/types';
import log from 'consola';

export default (ctx: Context): void => {
	Vue.config.productionTip = false;
	Vue.config.devtools = false;

	const { app } = ctx;
	const isProduction = process.env.NODE_ENV === 'production';

	// Doc: https://github.com/nuxt/consola
	// Inject in all context
	log.level = isProduction ? 1 : 5;
	Vue.prototype.$log = log;
	app.$log = Vue.prototype.$log;
	ctx.$log = Vue.prototype.$log;
};
