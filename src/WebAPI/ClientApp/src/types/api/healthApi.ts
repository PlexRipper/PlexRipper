import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map } from 'rxjs/operators';
import ResultDTO from '@dto/ResultDTO';

const apiPath = '/health';

export function getHealthStatus(): Observable<ResultDTO<string>> {
	const result = Axios.get<string>(`${apiPath}`, { timeout: 200 });
	return result.pipe(map((res: AxiosResponse) => res?.data));
}
