import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { AxiosResponse } from 'axios';
import type { ResultDTO } from '@interfaces';

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
		// Ensure we complete any API calls after the response has been received
		take(1),
	);
}
