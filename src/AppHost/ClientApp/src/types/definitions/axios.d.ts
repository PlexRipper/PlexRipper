import 'axios';

export {};

declare module 'axios' {
	interface AxiosRequestConfig {
		handleErrorsLocally?: boolean;
	}
}
