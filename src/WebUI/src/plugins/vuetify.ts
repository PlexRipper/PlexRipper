import '@mdi/font/css/materialdesignicons.css';
import 'material-design-icons-iconfont/dist/material-design-icons.css';
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
		customVariables: ['~/assets/scss/_variables.scss'],
		theme: {
			options: {
				customProperties: true,
			},
			dark: true,
		},
	});

	ctx.app.vuetify = vuetify;
	ctx.$vuetify = vuetify.framework;
};
