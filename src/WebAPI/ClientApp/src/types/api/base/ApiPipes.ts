import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AxiosResponse } from 'axios';
import type ResultDTO from '@dto/ResultDTO';

export function apiCheckPipe<T>(source$: Observable<AxiosResponse<T>>): Observable<ResultDTO<T>> {
	return source$.pipe(
		map((response) => response.data as ResultDTO<T>),
		map((res): ResultDTO<T> => {
			return {
				isSuccess: res.isSuccess,
				isFailed: res.isFailed,
				errors: res.errors,
				reasons: res.reasons,
				value: res.value,
			};
		}),
		// map((data) => data.value),
	);
}
