import { NuxtAppOptions } from '@nuxt/types';
import Log from 'consola';
import { baseApiUrl } from '@api/baseApi';

export default ({ $axios }: NuxtAppOptions): void => {
	$axios.onRequest((config) => {
		Log.debug(`Making request to ${config.url}`);
	});

	$axios.setBaseURL(baseApiUrl);
};
