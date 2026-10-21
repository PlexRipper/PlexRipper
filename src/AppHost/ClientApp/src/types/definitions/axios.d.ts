import 'axios';

export {};

declare module 'axios' {
	interface AxiosRequestConfig<D> {
		handleErrorsLocally?: boolean;
	}
}
