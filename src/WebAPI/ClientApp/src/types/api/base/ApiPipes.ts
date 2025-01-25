import Log from 'consola';
import type { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import type { AxiosResponse } from 'axios';
import type { BaseResultDTO, ErrorDTO } from '@dto';
import type { ResultDTO } from '@interfaces';
import { catchError, of } from 'rxjs';

export function apiCheckPipe<T extends object = BaseResultDTO>(
	source$: Observable<AxiosResponse<T>>,
): Observable<T extends BaseResultDTO ? BaseResultDTO : ResultDTO<T>> {
	return source$.pipe(
		map((res) => {
			// Handle generic ResultDTO<T> case
			if (Object.hasOwn(res.data, 'value')) {
				return toResultDTO<T>(res) as unknown as T extends BaseResultDTO ? BaseResultDTO : ResultDTO<T>;
			}

			// Handle BaseResultDTO case
			return res.data as unknown as T extends BaseResultDTO ? BaseResultDTO : ResultDTO<T>;
		}),
		catchError((error) => {
			Log.error('Error in API call', error);

			// Convert error response to ResultDTO
			return of(toResultDTO<never>(error.response)) as Observable<
				T extends BaseResultDTO ? BaseResultDTO : ResultDTO<T>
			>;
		}),
		// Ensure we complete any API calls after the response has been received
		take(1),
	);
}

// Convert AxiosResponse to ResultDTO<T>
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
		successes: result.successes,
		value: result.value,
		statusCode: res.status,
	};
}
