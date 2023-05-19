import Axios from 'axios-observable';

export default defineNuxtPlugin(() => {
	const config = useRuntimeConfig();
	Axios.defaults.baseURL = config.public.baseURL + config.public.baseApiPath;

	// Source: https://github.com/axios/axios/issues/41#issuecomment-484546457
	Axios.defaults.validateStatus = () => true;
	// Now error resolves in catch block rather than then block.
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

	// Inject Axios to context as $axios
	return {
		provide: {
			axios: () => {
				return Axios;
			},
		},
	};
});
