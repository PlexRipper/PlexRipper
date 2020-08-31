import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map } from 'rxjs/operators';

const apiPath = '/health';

export function getHealthStatus(): Observable<string> {
	const result: Observable<AxiosResponse> = Axios.get<string>(`${apiPath}`, { timeout: 200 });
	return result.pipe(map((res: AxiosResponse) => res?.data));
}
