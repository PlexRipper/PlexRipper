import { NuxtAppOptions } from '@nuxt/types';
import Log from 'consola';
import { baseApiUrl } from '@api/baseApi';
import Axios from 'axios-observable';
import GlobalService from '@service/globalService';

export default ({ $axios }: NuxtAppOptions): void => {
	$axios.onRequest((config) => {
		Log.debug(`Making request to ${config.url}`);
	});

	$axios.setBaseURL(baseApiUrl);

	Axios.defaults.baseURL = baseApiUrl;
	Axios.defaults.headers.get['Content-Type'] = 'application/json';
	Axios.defaults.headers.post['Content-Type'] = 'application/json';
	Log.debug('Finished setting up Axios');
	GlobalService.setAxiosReady();
};
