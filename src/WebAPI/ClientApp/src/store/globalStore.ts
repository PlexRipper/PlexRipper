import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { forkJoin, of, Subject } from 'rxjs';
import Log from 'consola';
import { switchMap, take, tap } from 'rxjs/operators';
import type IAppConfig from '@class/IAppConfig';
import type { I18nObjectType, ISetupResult } from '@interfaces';
import {
	useAccountStore,
	useAlertStore,
	useBackgroundJobsStore,
	useDialogStore,
	useDownloadStore,
	useFolderPathStore,
	useHelpStore,
	useLibraryStore,
	useLocalizationStore,
	useMediaStore,
	useNotificationsStore,
	useServerConnectionStore,
	useServerStore,
	useSettingsStore,
	useSignalrStore,
	useAuthenticationStore,
} from '@store';

export const useGlobalStore = defineStore('GlobalStore', () => {
	const state = reactive<{ config: IAppConfig; pageReadyObservable: Subject<boolean> }>({
		pageReadyObservable: new Subject<boolean>(),
		config: {} as IAppConfig,
	});
	const actions = {
		setupServices({ config, i18n }: { config: IAppConfig; i18n?: I18nObjectType }): Observable<ISetupResult[]> {
			Log.info('Starting Setup Process');

			state.config = config;
			Log.info('Runtime Config is ready - ' + config.version, config);

			return of(config).pipe(
				switchMap((config) =>
					forkJoin([
						useAccountStore().setup(),
						useAlertStore().setup(),
						useAuthenticationStore().setup(),
						useBackgroundJobsStore().setup(),
						useDialogStore().setup(),
						useDownloadStore().setup(),
						useFolderPathStore().setup(),
						useHelpStore().setup(),
						useLibraryStore().setup(),
						useLocalizationStore().setup(i18n),
						useMediaStore().setup(),
						useNotificationsStore().setup(),
						useServerConnectionStore().setup(),
						useServerStore().setup(),
						useSettingsStore().setup(),
						useSignalrStore().setup(config),
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
		setAppVersion(version: string): void {
			state.config.version = version;
		},
		$reset() {
			useAccountStore().$reset();
			useAlertStore().$reset();
			useAuthenticationStore().$reset();
			useBackgroundJobsStore().$reset();
			useDialogStore().$reset();
			useDownloadStore().$reset();
			useFolderPathStore().$reset();
			useHelpStore().$reset();
			useLibraryStore().$reset();
			useLocalizationStore().$reset();
			useMediaStore().$reset();
			useNotificationsStore().$reset();
			useServerConnectionStore().$reset();
			useServerStore().$reset();
			useSettingsStore().$reset();
			useSignalrStore().$reset();
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
