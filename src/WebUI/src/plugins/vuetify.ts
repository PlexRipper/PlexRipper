import 'material-design-icons-iconfont/dist/material-design-icons.css';
import '@mdi/font/css/materialdesignicons.css';
import Vue from 'vue';
import Vuetify from 'vuetify';
import { Context } from '@nuxt/types';

/*
 ** vuetify module configuration
 ** https://github.com/nuxt-community/vuetify-module
 */
export default (ctx: Context): void => {
	Vue.use(Vuetify);

	const vuetify = new Vuetify({
		theme: {
			options: {
				customProperties: true,
			},
			dark: true,
		},
		icons: {
			iconfont: 'mdi',
		},
	});

	ctx.app.vuetify = vuetify;
	ctx.$vuetify = vuetify.framework;
};
