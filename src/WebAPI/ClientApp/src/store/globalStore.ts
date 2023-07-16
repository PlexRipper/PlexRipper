import { acceptHMRUpdate, defineStore } from 'pinia';
import { forkJoin, Observable, of, Subject } from 'rxjs';
import Log from 'consola';
import { switchMap, take, tap } from 'rxjs/operators';
import IAppConfig from '@class/IAppConfig';
import I18nObjectType from '@interfaces/i18nObjectType';
import { MediaService, SignalrService } from '@service';
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

export const useGlobalStore = defineStore('GlobalStore', () => {
	const state = reactive<{ config: IAppConfig; pageReadyObservable: Subject<boolean> }>({
		pageReadyObservable: new Subject<boolean>(),
		config: {} as IAppConfig,
	});
	const actions = {
		setupServices({ config, i18n }: { config: IAppConfig; i18n: I18nObjectType }): Observable<any> {
			Log.info('Starting Setup Process');

			state.config = config;
			Log.info('Runtime Config is ready - ' + config.version, config);

			return of(config).pipe(
				switchMap((config) =>
					forkJoin([
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
					Log.info(`Page Setup has finished successfully`);
					state.pageReadyObservable.next(true);
				}),
				take(1),
			);
		},
	};
	const getters = {
		getPageSetupReady: computed((): Observable<boolean> => state.pageReadyObservable.asObservable()),
		getAppVersion: computed((): string => state.config?.version ?? '?'),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useGlobalStore, import.meta.hot));
}
