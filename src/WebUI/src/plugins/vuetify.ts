import Vue from 'vue';
import Vuetify from 'vuetify';
import { Context } from '@nuxt/types';
import 'material-design-icons-iconfont/dist/material-design-icons.css';
import '@mdi/font/css/materialdesignicons.css'; // Ensure you are using css-loader version "^2.1.1" ,

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
			dark: false,
		},
	});

	ctx.app.vuetify = vuetify;
	ctx.$vuetify = vuetify.framework;
};
