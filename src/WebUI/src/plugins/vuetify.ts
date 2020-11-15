import '@mdi/font/css/materialdesignicons.css';
import 'material-design-icons-iconfont/dist/material-design-icons.css';
import Vue from 'vue';
import Vuetify from 'vuetify';
import { Context } from '@nuxt/types';
import { Inject } from '@nuxt/types/app';

/*
 ** vuetify module configuration
 ** https://github.com/nuxt-community/vuetify-module
 */
export default (ctx: Context, inject: Inject): void => {
	Vue.use(Vuetify);

	const vuetify = new Vuetify({
		customVariables: ['~/assets/scss/_variables.scss'],
		theme: {
			themes: {
				light: {
					primary: '#ff0000',
				},
				dark: {
					primary: '#ff0000',
				},
			},
			options: {
				customProperties: true,
			},
			dark: true,
		},
	});

	ctx.app.vuetify = vuetify;
	ctx.$vuetify = vuetify.framework;

	inject('isDark', ctx.$vuetify.theme.dark);
	inject('getThemeClass', (): String => (ctx.$vuetify.theme.dark ? 'theme--dark' : 'theme--light'));
};
