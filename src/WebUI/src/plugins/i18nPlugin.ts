import { NuxtAppOptions } from '@nuxt/types';

export default (app: NuxtAppOptions): void => {
	app.$i18n = app.i18n;
};
