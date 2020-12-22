import Log from 'consola';
import AppConfig from '@interfaces/AppConfig';
import { ReplaySubject, Observable } from 'rxjs';
import { BaseService } from '@state/baseService';
import { RuntimeConfig } from '~/type_definitions/vueTypes';

export class GlobalService extends BaseService {
	private _axiosReady: ReplaySubject<any> = new ReplaySubject();
	private _configReady: ReplaySubject<AppConfig> = new ReplaySubject();

	public setAxiosReady(): void {
		Log.info('Axios is ready');
		this._axiosReady.next();
	}

	public setConfigReady(config: RuntimeConfig): void {
		if (process.client || process.static) {
			Log.info('Runtime Config is ready - ' + config.version);
			this._configReady.next(new AppConfig(config));
		} else {
			Log.error('setConfigReady => Process was neither client or static, was:', process);
		}
	}

	public getAxiosReady(): Observable<void> {
		return this._axiosReady.asObservable();
	}

	public getConfigReady(): Observable<AppConfig> {
		return this._configReady.asObservable();
	}
}

const globalService = new GlobalService();
export default globalService;
