import Vue from 'vue';
import { NuxtAppOptions } from '@nuxt/types';
import classNames from 'classnames';
import filesize from 'filesize';
import log from 'consola';

// eslint-disable-next-line import/no-unresolved
import { ClassValue } from 'classnames/types';

export default (app: NuxtAppOptions): void => {
	log.debug('Setup Vue filters');

	/*
	 * String filters
	 */
	Vue.filter('substr', (value: string, limit: number) => {
		if (value?.length > limit) {
			return `${value.substring(0, limit)}...`;
		}
		return value;
	});

	/*
	 * ClassNames filters
	 */

	Vue.prototype.$classNames = (...classes: ClassValue[]): string => classNames(...classes);
	app.$classNames = Vue.prototype.$classNames;

	/* Filesize filters */
	Vue.filter('formatFilesize', (value: number) => filesize(value));
};
