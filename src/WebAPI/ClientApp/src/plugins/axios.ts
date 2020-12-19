import Log from 'consola';
import { AxiosResponse } from 'axios';
import { Context } from '@nuxt/types';
import Axios from 'axios-observable';
import GlobalService from '@state/globalService';
import Result from 'fluent-type-results';

export default (ctx: Context): void => {
	ctx.$axios.onRequest((config) => {
		Log.debug(`Making request to ${config.url}`, config);
	});

	GlobalService.getConfigReady().subscribe(() => {
		if (process.client || process.static) {
			const host = window.location.hostname;
			const baseURL = 'http://' + host + ':7000';
			ctx.$axios.setBaseURL(baseURL);
			Axios.defaults.baseURL = baseURL;
			Log.debug('Finished setting up Axios');
		}
		GlobalService.setAxiosReady();
	});

	Axios.defaults.headers.get['Content-Type'] = 'application/json';
	Axios.defaults.headers.get['Access-Control-Allow-Origin'] = '*';

	Axios.defaults.headers.post['Content-Type'] = 'application/json';
	Axios.defaults.headers.post['Access-Control-Allow-Origin'] = '*';
	// Axios.defaults.withCredentials = false;

	Axios.interceptors.response.use(
		(response): AxiosResponse<Result<any>> => {
			if (response?.data && (response.data as Result)) {
				const result = response.data as Result;
				// eslint-disable-next-line no-prototype-builtins
				if (result.hasOwnProperty('statusCode')) {
					result.statusCode = response.status;
				}
			}
			return response;
		},
		(error) => {
			if (error?.response?.data as Result) {
				(error.response.data as Result).statusCode = error.response.status;
			}
			return error.response;
		},
	);
};
