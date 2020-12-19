import Log from 'consola';
import IAppConfig from '@interfaces/IAppConfig';
import { ReplaySubject, Observable } from 'rxjs';
import { BaseService } from '@state/baseService';
import { RuntimeConfig } from '~/type_definitions/vueTypes';

export class GlobalService extends BaseService {
	private _axiosReady: ReplaySubject<any> = new ReplaySubject();
	private _configReady: ReplaySubject<IAppConfig> = new ReplaySubject();

	public setAxiosReady(): void {
		Log.info('Axios is ready');
		this._axiosReady.next();
	}

	public setConfigReady(config: RuntimeConfig): void {
		if (process.client || process.static) {
			const appConfig: IAppConfig = {
				nodeEnv: config.nodeEnv,
				baseURL: window.location.origin,
				baseApiUrl: window.location.origin + '/api',
			};

			Log.info('Runtime Config is ready');
			this._configReady.next(appConfig);
		} else {
			Log.error('setConfigReady => Process was neither client or static, was:', process);
		}
	}

	public getAxiosReady(): Observable<void> {
		return this._axiosReady.asObservable();
	}

	public getConfigReady(): Observable<IAppConfig> {
		return this._configReady.asObservable();
	}
}

const globalService = new GlobalService();
export default globalService;
