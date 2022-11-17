import Log from 'consola';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { catchError, Observable, of } from 'rxjs';
import Axios, { AxiosObservable } from 'axios-observable';
import { AxiosResponse } from 'axios';
import { AlertService } from '@service';
import ResultDTO from '@dto/ResultDTO';

export function preApiRequest(logText: string, fnName: string, data: any | string = 'none'): void {
	Log.debug(`${logText} ${fnName} => sending request:`, data);
}

export function checkForError<T = any>(
	logText?: string,
	fnName?: string,
): (source$: AxiosObservable<ResultDTO<T>>) => Observable<ResultDTO<T>> {
	return (source$) =>
		source$.pipe(
			catchError((error: any) => {
				Log.fatal('FATAL NETWORK ERROR: axiosErrorToResultDTO', error);
				// TODO Check wat the error contains incase of network failure and continue based on that
				return of({} as AxiosResponse<ResultDTO<T>>);
			}),
			switchMap((response: AxiosResponse<ResultDTO<T>>): Observable<ResultDTO<T>> => {
				return of(response?.data);
			}),
			tap((data) => Log.trace(`${logText}${fnName} response:`, data)),
			// Ensure we complete any API calls after the response has been received
			take(1),
		);
}

export function checkResponse<T = ResultDTO | ResultDTO<void> | undefined>(
	response: AxiosObservable<T>,
	logText: string,
	fnName: string,
): Observable<T> {
	// Pipe response
	return response.pipe(
		tap((res) => {
			if (res && res.status && !res.status.toString().startsWith('2')) {
				const response = res.data;
				switch (res.status) {
					case 400:
						Log.error(`${logText}${fnName} => Bad Request (400) from response:`, res.request);
						AlertService.showAlert({ id: 0, title: 'Bad Request (400)', text: '', result: response });
						return;

					case 404:
						Log.error(`${logText}${fnName} => Not Found (404) from response:`, res.request);
						AlertService.showAlert({ id: 0, title: 'Not Found (404)', text: '', result: response });
						return;

					case 500:
						Log.error(`${logText}${fnName} => Internal Server Error (500) from response:`, response);
						AlertService.showAlert({ id: 0, title: 'Internal Server Error (500)', text: '', result: response });
						return;

					default:
						Log.error(`${logText}${fnName} => Unknown Error (Status ${res.status}) from response:`, res.request);
						AlertService.showAlert({ id: 0, title: 'Unknown Error', text: '', result: response });
						break;
				}
			}
		}),
		map((res) => res?.data),
		tap((data) => Log.trace(`${logText}${fnName} response:`, data)),
	);
}
