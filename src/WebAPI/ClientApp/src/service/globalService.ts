import Log from 'consola';
import { Context } from '@nuxt/types';
import { ReplaySubject, Observable, forkJoin } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { ObservableStore } from '@codewithdan/observable-store';
import AppConfig from '@class/AppConfig';
import {
	ConfirmationSettingsDTO,
	DateTimeSettingsDTO,
	DisplaySettingsDTO,
	DownloadManagerSettingsDTO,
	DownloadTaskCreationProgress,
	GeneralSettingsDTO,
	LanguageSettingsDTO,
	ServerSettingsDTO,
} from '@dto/mainApi';
import IStoreState from '@interfaces/service/IStoreState';
import * as Service from '@service';
import { RuntimeConfig } from '~/type_definitions/vueTypes';
import ISetupResult from '@interfaces/service/ISetupResult';

export class GlobalService {
	private _axiosReady: ReplaySubject<void> = new ReplaySubject();
	private _configReady: ReplaySubject<AppConfig> = new ReplaySubject();
	private _pageSetupReady: ReplaySubject<void> = new ReplaySubject();
	private _defaultStore: IStoreState = {
		accounts: [],
		servers: [],
		libraries: [],
		mediaUrls: [],
		notifications: [],
		folderPaths: [],
		alerts: [],
		helpIdDialog: '',
		downloadTaskUpdateList: [],
		// Progress Service
		fileMergeProgressList: [],
		inspectServerProgress: [],
		syncServerProgress: [],
		libraryProgress: [],
		downloadTaskCreationProgress: {} as DownloadTaskCreationProgress,
		serverDownloads: [],
		// Settings Modules
		dateTimeSettings: {} as DateTimeSettingsDTO,
		downloadManagerSettings: {} as DownloadManagerSettingsDTO,
		generalSettings: {} as GeneralSettingsDTO,
		languageSettings: {} as LanguageSettingsDTO,
		displaySettings: {} as DisplaySettingsDTO,
		confirmationSettings: {} as ConfirmationSettingsDTO,
		serverSettings: {} as ServerSettingsDTO,
	};

	private _serviceKeys: string[] = [];
	private _doneServiceKeys: string[] = [];

	public setAxiosReady(): void {
		Log.info('Axios is ready');
		this._axiosReady.next();
	}

	public initializeState() {
		ObservableStore.initializeState(this._defaultStore);
	}

	public setupObservable(nuxtContext: Context): Observable<ISetupResult[]> {
		this.initializeState();
		this.setConfigReady(nuxtContext.$config);
		const appConfig = new AppConfig(nuxtContext.$config);
		const sources = [
			Service.ProgressService.setup(nuxtContext),
			Service.DownloadService.setup(nuxtContext),
			Service.ServerService.setup(nuxtContext),
			Service.MediaService.setup(nuxtContext),
			Service.SettingsService.setup(nuxtContext),
			Service.NotificationService.setup(nuxtContext),
			Service.FolderPathService.setup(nuxtContext),
			Service.LibraryService.setup(nuxtContext),
			Service.AccountService.setup(nuxtContext),
			Service.SignalrService.setup(nuxtContext, appConfig),
			Service.HelpService.setup(nuxtContext),
			Service.AlertService.setup(nuxtContext),
		];
		return forkJoin(sources).pipe(
			tap((results: ISetupResult[]) => {
				const percentage = Number(((results.length / sources.length) * 100).toFixed(2));
				Log.debug(`Setup progress: ${percentage}%`);
			}),
		);
	}

	public setup(nuxtContext: Context): void {
		this.setupObservable(nuxtContext).subscribe({
			complete: () => {
				Log.info('Page Setup has finished');
				this._pageSetupReady.next();
			},
		});
	}

	public resetStore(): void {
		ObservableStore.resetState(this._defaultStore);
	}

	private setConfigReady(config: RuntimeConfig): void {
		if (process.client || process.static) {
			Log.info('Runtime Config is ready - ' + config.version, config);
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

	public getPageSetupReady(): Observable<void> {
		return this._pageSetupReady.pipe(take(1));
	}
}

const globalService = new GlobalService();
export default globalService;
