import Log from 'consola';
import Axios from 'axios-observable';
import IAppConfig from '@class/IAppConfig';

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
};

export function setConfigInAxios(config: IAppConfig) {
	Log.info('Axios BaseApiUrl: ' + config.baseApiUrl);
	Axios.defaults.baseURL = config.baseApiUrl;
}
