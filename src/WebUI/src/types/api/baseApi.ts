import Log from 'consola';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AxiosResponse } from 'axios';
import Result from 'fluent-type-results';

export const baseUrl = 'https://localhost:5001';
// export const baseUrlHttp = "http://localhost:5000";

export const baseApiUrl = `${baseUrl}/api`;

export const signalRDownloadProgressUrl = `${baseUrl}/download/progress`;

export function checkResponse<T>(response: Observable<AxiosResponse<Result<T>>>, logText: string, fnName: string): Observable<T> {
	// Check statusCode
	response.subscribe((res) => {
		if (res.status !== 200) {
			switch (res.status) {
				case 400:
					Log.error(`${logText} ${fnName} => Bad Request with request:`, res.request);
					return;

				case 404:
					Log.error(`${logText} ${fnName} => Not Found with request:`, res.request);
					return;

				default:
					Log.error(`${logText} ${fnName} => Unknown Error with request:`, res.request);
					break;
			}
		}
	});

	// Pipe response
	return response.pipe(
		map((res: AxiosResponse) => res.data),
		tap((data) => Log.debug(`${logText} ${fnName} response:`, data)),
	);
}
