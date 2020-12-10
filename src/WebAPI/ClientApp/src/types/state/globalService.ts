import Log from 'consola';
import { ReplaySubject, Observable } from 'rxjs';
import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';

export class GlobalService extends ObservableStore<StoreState> {
	private _axiosReady: ReplaySubject<any> = new ReplaySubject();

	public constructor() {
		super({ trackStateHistory: true });

		ObservableStore.initializeState({
			servers: [],
			downloads: [],
		} as StoreState);
	}

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
