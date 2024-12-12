import type { Observable } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';
import type { AxiosResponse } from 'axios';
import type { ResultDTO } from '@interfaces';
import { useGlobalStore } from '@store';

export function apiCheckPipe<T>(source$: Observable<AxiosResponse<T>>): Observable<ResultDTO<T>> {
	return source$.pipe(
		tap((res) => useGlobalStore().setAppVersion(res.headers['x-plexripper-version'])),
		map((res) => res.data as ResultDTO<T>),
		map((res): ResultDTO<T> => {
			return {
				isSuccess: res.isSuccess,
				isFailed: res.isFailed,
				errors: res.errors,
				reasons: res.reasons,
				value: res.value,
				successes: res.successes,
			};
		}),
		// Ensure we complete any API calls after the response has been received
		take(1),
	);
}
