import Log from 'consola';
import { Context } from '@nuxt/types';
import AppConfig from '@class/AppConfig';
import { ReplaySubject, Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { ObservableStoreSettings } from '@codewithdan/observable-store/interfaces';
import { ObservableStore } from '@codewithdan/observable-store';
import { SettingsModel } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import {
	BaseService,
	ProgressService,
	DownloadService,
	ServerService,
	SettingsService,
	NotificationService,
	AccountService,
	SignalrService,
	AlertService,
} from '@service';
import { RuntimeConfig } from '~/type_definitions/vueTypes';

export class GlobalService extends BaseService {
	private _axiosReady: ReplaySubject<void> = new ReplaySubject();
	private _configReady: ReplaySubject<AppConfig> = new ReplaySubject();

	constructor() {
		super({} as ObservableStoreSettings);
	}

	public setAxiosReady(): void {
		Log.info('Axios is ready');
		this._axiosReady.next();
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		ObservableStore.initializeState({
			accounts: [],
			servers: [],
			downloads: [],
			libraries: [],
			mediaUrls: [],
			notifications: [],
			alerts: [],
			helpIdDialog: '',
			settings: {} as SettingsModel,
			fileMergeProgressList: [],
			downloadTaskUpdateList: [],
		} as IStoreState);

		SignalrService.setup(nuxtContext);
		AccountService.setup(nuxtContext);
		SettingsService.setup(nuxtContext);
		ServerService.setup(nuxtContext);
		DownloadService.setup(nuxtContext);
		ProgressService.setup(nuxtContext);
		NotificationService.setup(nuxtContext);
		AlertService.setup(nuxtContext);
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
		return this._axiosReady.pipe(take(1));
	}

	public getConfigReady(): Observable<AppConfig> {
		return this._configReady.pipe(take(1));
	}
}

const globalService = new GlobalService();
export default globalService;
