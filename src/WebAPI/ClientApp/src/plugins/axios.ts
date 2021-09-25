import Log from 'consola';
import Axios from 'axios-observable';
import { GlobalService } from '@service';

export default (): void => {
	// Source: https://github.com/axios/axios/issues/41#issuecomment-484546457
	Axios.defaults.validateStatus = () => true;
	// Source: https://github.com/axios/axios/issues/41#issuecomment-386762576
	Axios.interceptors.response.use(
		(config) => {
			return config;
		},
		(error) => {
			// Prevents 400 & 500 status code throwing exceptions
			return Promise.reject(error);
		},
	);

	GlobalService.getConfigReady().subscribe((config) => {
		Log.info('Axios BaseApiUrl: ' + config.baseApiUrl);
		Axios.defaults.baseURL = config.baseApiUrl;
		GlobalService.setAxiosReady();
	});
};
