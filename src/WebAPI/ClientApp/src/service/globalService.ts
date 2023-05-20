import Log from 'consola';

import { ObservableStore } from '@codewithdan/observable-store';
import { forkJoin, Observable, of } from 'rxjs';
import { filter, map, switchMap, take, tap } from 'rxjs/operators';
import DefaultState from '@const/default-state';
import IAppConfig from '@class/IAppConfig';
import IStoreState from '@interfaces/service/IStoreState';
import {
	AccountService,
	AlertService,
	BackgroundJobsService,
	BaseService,
	DownloadService,
	FolderPathService,
	HelpService,
	LibraryService,
	MediaService,
	NotificationService,
	ProgressService,
	ServerConnectionService,
	ServerService,
	SettingsService,
	SignalrService,
} from '@service';

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

	public setup(config: IAppConfig): Observable<any> {
		Log.info('Starting Setup Process');

		super.setup(config);

		return of(this._appConfig).pipe(
			tap((config) => this.setConfigReady(config)),
			switchMap((config) =>
				forkJoin([
					ProgressService.setup(),
					DownloadService.setup(),
					ServerService.setup(),
					ServerConnectionService.setup(),
					BackgroundJobsService.setup(),
					MediaService.setup(),
					SettingsService.setup(),
					NotificationService.setup(),
					FolderPathService.setup(),
					LibraryService.setup(),
					AccountService.setup(),
					SignalrService.setup(config),
					HelpService.setup(),
					AlertService.setup(),
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

	public setupServices(config: IAppConfig): void {
		this.setup(config).subscribe();
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
