import Log from 'consola';
import { ReplaySubject, Observable } from 'rxjs';
import { BaseService } from '@state/baseService';

export class GlobalService extends BaseService {
	private _axiosReady: ReplaySubject<any> = new ReplaySubject();

	public setAxiosReady(): void {
		Log.debug('Axios is ready');
		this._axiosReady.next();
	}

	public getAxiosReady(): Observable<void> {
		return this._axiosReady.asObservable();
	}
}

const globalService = new GlobalService();
export default globalService;
