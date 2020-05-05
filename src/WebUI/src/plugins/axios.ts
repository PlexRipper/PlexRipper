import { NuxtAppOptions } from '@nuxt/types';
import Log from 'consola';
export default ({ $axios }: NuxtAppOptions): void => {
	$axios.onRequest((config) => {
		Log.debug(`Making request to ${config.url}`);
	});

	// $axios.setBaseURL('http://localhost:5001/api');
};
