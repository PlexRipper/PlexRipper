import Log from 'consola';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AxiosResponse } from 'axios';
import { AxiosObservable } from 'axios-observable/dist/axios-observable.interface';
import { AlertService } from '@service';
import ResultDTO from '@dto/ResultDTO';

export function preApiRequest(logText: string, fnName: string, data: any | string = 'none'): void {
	Log.debug(`${logText} ${fnName} => sending request:`, data);
}

export function checkResponse<T = ResultDTO>(response: AxiosObservable<T>, logText: string, fnName: string): Observable<T> {
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
		map((res: AxiosResponse) => res?.data),
		tap((data) => Log.debug(`${logText}${fnName} response:`, data)),
	);
}
