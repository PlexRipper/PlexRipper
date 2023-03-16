import Log from 'consola';

import {ObservableStore} from '@codewithdan/observable-store';
import DefaultState from '@const/default-state';
import IAppConfig from '@class/IAppConfig';
import IStoreState from '@interfaces/service/IStoreState';
import * as Service from '@service';
import {BaseService} from '@service';
import {getBaseURL} from '@api-urls';
import {forkJoin, Observable, of} from 'rxjs';
import {filter, map, switchMap, take, tap} from 'rxjs/operators';

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

    public setup(): Observable<any> {

        const publicConfig = useRuntimeConfig().public;
        const baseUrl = getBaseURL(publicConfig.nodeEnv === 'production');
        const appConfig: IAppConfig = {
            version: publicConfig.version,
            nodeEnv: publicConfig.nodeEnv,
            isProduction: publicConfig.nodeEnv === 'production',
            baseURL: baseUrl,
            baseApiUrl: `${baseUrl}/api`,
        };
        super.setup(appConfig);

        return of(this._appConfig).pipe(
            tap((config) => this.setConfigReady(config)),
            switchMap((config) =>
                forkJoin([
                    Service.ProgressService.setup(),
                    Service.DownloadService.setup(),
                    Service.ServerService.setup(),
                    Service.ServerConnectionService.setup(),
                    Service.BackgroundJobsService.setup(),
                    Service.MediaService.setup(),
                    Service.SettingsService.setup(),
                    Service.NotificationService.setup(),
                    Service.FolderPathService.setup(),
                    Service.LibraryService.setup(),
                    Service.AccountService.setup(),
                    Service.SignalrService.setup(config),
                    Service.HelpService.setup(),
                    Service.AlertService.setup(),
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

    public setupServices(): void {
        this.setup().subscribe();
    }

    public resetStore(): void {
        ObservableStore.resetState(DefaultState);
    }

    private setConfigReady(appConfig: IAppConfig): void {
        if (process.client || process.static) {
            Log.info('Runtime Config is ready - ' + appConfig.version, appConfig);
            this.setState({config: appConfig}, 'Set App Config on page load');
        } else {
            Log.error('setConfigReady => Process was neither client or static, was:', process);
        }
    }

    private setPageSetupReady(): void {
        Log.info('Page Setup has finished');
        this.setState({pageReady: true}, 'Set Page Ready Setup');
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
