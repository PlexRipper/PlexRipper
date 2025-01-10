import Log from 'consola';
import type { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { AxiosResponse } from 'axios';
import type { ResultDTO } from '@interfaces';
import { catchError, of } from 'rxjs';
import type { ErrorDTO } from '@dto';

export function apiCheckPipe<T>(source$: Observable<AxiosResponse<T>>): Observable<ResultDTO<T>> {
	return source$.pipe(
		map((res) => toResultDTO<T>(res)),
		catchError((error) => {
			Log.error('Error in API call', error);
			return of(toResultDTO<never>(error.response));
		}),
		// Ensure we complete any API calls after the response has been received
		take(1),
	);
}

function toResultDTO<T>(res?: AxiosResponse): ResultDTO<T> {
	if (!res) {
		const error: ErrorDTO = {
			message: 'Internal Server Error',
			reasons: [],
			metadata: {},
		};
		return {
			isSuccess: false,
			isFailed: true,
			errors: [error],
			reasons: [error],
			successes: [],
			statusCode: 999,
		};
	}
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
