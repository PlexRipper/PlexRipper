import Log from 'consola';
import type { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { AxiosResponse } from 'axios';
import type { ResultDTO } from '@interfaces';
import { catchError, of } from 'rxjs';
import type { BaseResultDTO, ErrorDTO } from '@dto';

export function apiCheckPipe<T = BaseResultDTO>(
	source$: Observable<AxiosResponse<T>>,
): Observable<T extends BaseResultDTO ? T : ResultDTO<T>> {
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

function toResultDTO<T = void>(res?: AxiosResponse): ResultDTO<T> {
	if (!res) {
		const error: ErrorDTO = {
			message: 'Internal Server Error',
			reasons: [],
			metadata: {},
		};
		return {
			isSuccess: false,
			errors: [error],
			successes: [],
			statusCode: 999,
		};
	}
	const result = res.data as ResultDTO<T>;
	return {
		isSuccess: result.isSuccess,
		errors: result.errors,
		value: result.value,
		successes: result.successes,
		statusCode: res.status,
	};
}
