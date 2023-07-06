import Log from 'consola';

import { ObservableStore } from '@codewithdan/observable-store';
import { forkJoin, Observable, of } from 'rxjs';
import { filter, map, switchMap, take, tap } from 'rxjs/operators';
import DefaultState from '@const/default-state';
import IAppConfig from '@class/IAppConfig';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService, MediaService, ProgressService, SignalrService } from '@service';
import {
	useServerStore,
	useLibraryStore,
	useDownloadStore,
	useAccountStore,
	useNotificationsStore,
	useFolderPathStore,
	useServerConnectionStore,
	useSettingsStore,
	useBackgroundJobsStore,
	useHelpStore,
	useAlertStore,
	useLocalizationStore,
} from '#imports';
import I18nObjectType from '@interfaces/i18nObjectType';

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

	public setupServices({ config, i18n }: { config: IAppConfig; i18n: I18nObjectType }): Observable<any> {
		Log.info('Starting Setup Process');

		super.setup(config);

		return of(this._appConfig).pipe(
			tap((config) => this.setConfigReady(config)),
			switchMap((config) =>
				forkJoin([
					ProgressService.setup(),
					MediaService.setup(),
					SignalrService.setup(config),
					useAccountStore().setup(),
					useDownloadStore().setup(),
					useFolderPathStore().setup(),
					useLibraryStore().setup(),
					useNotificationsStore().setup(),
					useServerConnectionStore().setup(),
					useServerStore().setup(),
					useSettingsStore().setup(),
					useBackgroundJobsStore().setup(),
					useHelpStore().setup(),
					useAlertStore().setup(),
					useLocalizationStore().setup(i18n),
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

export default new GlobalService();
