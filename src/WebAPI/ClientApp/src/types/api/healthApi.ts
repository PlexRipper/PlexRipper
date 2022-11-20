import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map } from 'rxjs/operators';
import ResultDTO from '@dto/ResultDTO';
import { HEALTH_RELATIVE_PATH } from '@api-urls';

export function getHealthStatus(): Observable<ResultDTO<string>> {
	const result = Axios.get<string>(`${HEALTH_RELATIVE_PATH}`, { timeout: 200 });
	return result.pipe(map((res: AxiosResponse) => res?.data));
}
