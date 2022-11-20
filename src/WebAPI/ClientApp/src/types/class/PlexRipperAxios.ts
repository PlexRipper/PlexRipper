import Axios from 'axios-observable';
import { checkForError } from '@api/baseApi';
import ResultDTO from '@dto/ResultDTO';

export interface PlexRipperAxiosGet {
	url: string;
	apiCategory?: string;
	apiName?: string;
	params?: any;
}

export interface PlexRipperAxiosPut extends PlexRipperAxiosGet {
	data?: any;
}

export interface PlexRipperAxiosPost extends PlexRipperAxiosGet {
	data?: any;
}

export default class PlexRipperAxios {
	public static get<T = any>({ url, apiCategory, apiName, params }: PlexRipperAxiosGet) {
		return Axios.get<ResultDTO<T>>(url, params).pipe(checkForError<T>(apiCategory, apiName));
	}

	public static put<T = any>({ url, data, apiCategory, apiName }: PlexRipperAxiosPut) {
		return Axios.put<ResultDTO<T>>(url, data).pipe(checkForError<T>(apiCategory, apiName));
	}

	public static post<T = any>({ url, data, apiCategory, apiName }: PlexRipperAxiosPost) {
		return Axios.post<ResultDTO<T>>(url, data).pipe(checkForError<T>(apiCategory, apiName));
	}

	public static delete<T = any>({ url, apiCategory, apiName }: PlexRipperAxiosGet) {
		return Axios.delete<ResultDTO<T>>(url).pipe(checkForError<T>(apiCategory, apiName));
	}
}
