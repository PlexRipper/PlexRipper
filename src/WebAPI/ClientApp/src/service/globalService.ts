import Log from 'consola';
import { Context } from '@nuxt/types';
import { Observable, forkJoin, of } from 'rxjs';
import { filter, map, switchMap, take, tap } from 'rxjs/operators';
import { ObservableStore } from '@codewithdan/observable-store';
import DefaultState from '@const/default-state';
import IAppConfig from '@class/IAppConfig';
import IStoreState from '@interfaces/service/IStoreState';
import * as Service from '@service';
import { setConfigInAxios } from '~/plugins/axios';
import { setLogConfig } from '~/plugins/setup';
import { BaseService } from '@service';
import { getBaseURL } from '@api-urls';

export class GlobalService extends BaseService {
	public constructor() {
		super('GlobalService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					config: state.config,
					pageReady: state.pageReady,
				};
			},
		});
	}

	public setupObservable(nuxtContext: Context): Observable<any> {
		const $config = nuxtContext.$config;
		const baseUrl = getBaseURL($config.nodeEnv === 'production');
		return of({
			version: $config.version,
			nodeEnv: $config.nodeEnv,
			isProduction: $config.nodeEnv === 'production',
			baseURL: baseUrl,
			baseApiUrl: `${baseUrl}/api`,
		} as IAppConfig).pipe(
			tap((config) => setLogConfig(config)),
			tap((config) => this.setConfigReady(config)),
			tap((config) => setConfigInAxios(config)),
			switchMap((config) =>
				forkJoin([
					Service.ProgressService.setup(nuxtContext),
					Service.DownloadService.setup(nuxtContext),
					Service.ServerService.setup(nuxtContext),
					Service.MediaService.setup(nuxtContext),
					Service.SettingsService.setup(nuxtContext),
					Service.NotificationService.setup(nuxtContext),
					Service.FolderPathService.setup(nuxtContext),
					Service.LibraryService.setup(nuxtContext),
					Service.AccountService.setup(nuxtContext),
					Service.SignalrService.setup(nuxtContext, config),
					Service.HelpService.setup(nuxtContext),
					Service.AlertService.setup(nuxtContext),
				]),
			),
			tap((results) => {
				if (results.some((result) => !result.isSuccess)) {
					for (const result of results) {
						if (!result.isSuccess) {
							Log.error(`Service ${result.name} has a failed setup process`);
						}
					}
				}
				Log.info(`Setup progress has finished successfully`);
				this.setPageSetupReady();
			}),
			take(1),
		);
	}

	public setupServices(nuxtContext: Context): void {
		this.setupObservable(nuxtContext).subscribe();
	}

	public resetStore(): void {
		ObservableStore.resetState(DefaultState);
	}

	private setConfigReady(appConfig: IAppConfig): void {
		if (process.client || process.static) {
			Log.info('Runtime Config is ready - ' + appConfig.version, appConfig);
			this.setState({ config: appConfig }, 'Set App Config on page load');
		} else {
			Log.error('setConfigReady => Process was neither client or static, was:', process);
		}
	}

	private setPageSetupReady(): void {
		Log.info('Page Setup has finished');
		this.setState({ pageReady: true }, 'Set Page Ready Setup');
	}

	public getConfigReady(): Observable<IAppConfig> {
		return this.stateChanged.pipe(
			map((value) => value.config),
			take(1),
		);
	}

	public getPageSetupReady(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((value) => value?.pageReady ?? false),
			filter((pageReady) => pageReady),
		);
	}
}

const globalService = new GlobalService();
export default globalService;
