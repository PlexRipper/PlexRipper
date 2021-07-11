import Log from 'consola';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AxiosResponse } from 'axios';
import Result from 'fluent-type-results';

export function preApiRequest(logText: string, fnName: string, data: any | string = 'none'): void {
	Log.debug(`${logText} ${fnName} => sending request:`, data);
}

export function checkResponse<T>(
	response: Observable<AxiosResponse<Result<T> | Result>>,
	logText: string,
	fnName: string,
): Observable<T> {
	// Pipe response
	return response.pipe(
		tap((res: AxiosResponse<Result<T>>) => {
			if (res?.status && res?.status !== 200) {
				switch (res.status) {
					case 400:
						Log.error(`${logText}${fnName} => Bad Request (400) from response:`, res.request);
						return;

					case 404:
						Log.error(`${logText}${fnName} => Not Found (404) from response:`, res.request);
						return;

					case 500:
						Log.error(`${logText}${fnName} => Internal Server Error (500) from response:`, res.request.response);
						return;

					default:
						Log.error(`${logText}${fnName} => Unknown Error (Status ${res.status}) from response:`, res.request);
						break;
				}
			}
		}),
		map((res: AxiosResponse) => {
			if (res?.data?.value) {
				return res.data.value;
			}
			return res?.data;
		}),
		tap((data) => Log.debug(`${logText}${fnName} response:`, data)),
	);
}
