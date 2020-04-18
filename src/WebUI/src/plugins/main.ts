import Vue from 'vue';
import { NuxtAppOptions } from '@nuxt/types';
import setupFilters from './filters';

export default (app: NuxtAppOptions): void => {
	if (!app.$keycloak.authenticated) {
		app.$log.error('Is not authenticated, abort main.ts setup process');
		return;
	}
	app.$log.debug('START MAIN.TS');

	setupFilters(app);

	// set lodash for the whole application
	Vue.prototype._ = require('lodash');
};
