import IPlexAccount from '@dto/IPlexAccount';
import { Observable } from 'rxjs';
import { AxiosResponse } from 'axios';
import Axios from 'axios-observable';
import { map } from 'rxjs/operators';

const apiPath = '/settings';

export function getActiveAccount(): Observable<IPlexAccount | null> {
	const result: Observable<AxiosResponse> = Axios.get<IPlexAccount>(`${apiPath}/activeaccount`);
	return result.pipe(map((res: AxiosResponse) => res.data));
}

export function setActiveAccount(accountId: number): Observable<IPlexAccount | null> {
	const result: Observable<AxiosResponse> = Axios.put<IPlexAccount>(`${apiPath}/activeaccount/${accountId}`);
	return result.pipe(map((res: AxiosResponse) => res.data));
}
