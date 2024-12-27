import type { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { AxiosResponse } from 'axios';
import type { ResultDTO } from '@interfaces';
import { catchError, of } from 'rxjs';

export function apiCheckPipe<T>(source$: Observable<AxiosResponse<T>>): Observable<ResultDTO<T>> {
	return source$.pipe(
		map((res) => toResultDTO<T>(res)),
		catchError((error) => of(toResultDTO<never>(error.response))),
		// Ensure we complete any API calls after the response has been received
		take(1),
	);
}

function toResultDTO<T>(res: AxiosResponse): ResultDTO<T> {
	const result = res.data as ResultDTO<T>;
	return {
		isSuccess: result.isSuccess,
		isFailed: result.isFailed,
		errors: result.errors,
		reasons: result.reasons,
		value: result.value,
		successes: result.successes,
		statusCode: res.status,
	};
}
