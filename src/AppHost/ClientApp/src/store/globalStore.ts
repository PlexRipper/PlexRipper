import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import type { Subject, Observable } from 'rxjs';
import { catchError, ReplaySubject, forkJoin, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import type IAppConfig from '@class/IAppConfig';
import type { ISetupResult } from '@interfaces';
import {
	useAccountStore,
	useAlertStore,
	useBackgroundJobsStore,
	useDialogStore,
	useDownloadStore,
	useFolderPathStore,
	useHelpStore,
	useIntegrationStore,
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
import { cloneDeep } from 'lodash-es';

interface IAppConfigStoreState {
	version: string;
	config: IAppConfig;
	pageReadyObservable: Subject<boolean>;
}

export const useGlobalStore = defineStore('GlobalStore', () => {
	const defaultState: IAppConfigStoreState = {
		version: '?',
		config: {} as IAppConfig,
		pageReadyObservable: new ReplaySubject<boolean>(),
	};

	const state = reactive<IAppConfigStoreState>(cloneDeep(defaultState));

	const actions = {
		setupServices({ config }: { config: IAppConfig }): Observable<ISetupResult[]> {
			Log.info('Runtime Config is ready:', config);

			state.config = config;

			return actions.setup();
		},
		setup() {
			return useAuthenticationStore().setup().pipe(
				tap(() => state.pageReadyObservable.next(false)),
				switchMap((authResult): Observable<ISetupResult[]> => {
					if (!authResult.isSuccess) {
						state.pageReadyObservable.next(true);
						return of([authResult]);
					}
					return forkJoin([
						useAccountStore().setup(),
						useAlertStore().setup(),
						useBackgroundJobsStore().setup(),
						useDialogStore().setup(),
						useDownloadStore().setup(),
						useFolderPathStore().setup(),
						useHelpStore().setup(),
						useLibraryStore().setup(),
						useLocalizationStore().setup(),
						useMediaStore().setup(),
						useNotificationsStore().setup(),
						useServerConnectionStore().setup(),
						useServerStore().setup(),
						useSettingsStore().setup(),
						useSignalrStore().setup(),
					]).pipe(
						switchMap((results) => {
							// Run integration store setup after settings are loaded
							return useIntegrationStore().setup().pipe(
								switchMap((integrationResult) => of([...results, integrationResult])),
							);
						}),
					);
				}),
				catchError((error) => {
					if (error === 'Unauthorized') {
						return of([{ name: 'PageSetup', isSuccess: true }]);
					}
					Log.error('Page Setup has failed:', error);
					return of([{ name: 'PageSetup', isSuccess: false }]);
				}),
				tap((results) => {
					state.pageReadyObservable.next(true);
					if (results.some((result) => !result.isSuccess)) {
						for (const result of results) {
							if (!result.isSuccess) {
								Log.error(`Service ${result.name} has a failed setup process`, result);
							}
						}
					}
				}),
			);
		},
		setAppVersion(version: string): void {
			if (!version || state.version === version) {
				return;
			}
			Log.info('Reaparr App Version:', version);
			state.version = version;
		},
		$reset() {
			useAccountDialogStore().$reset();
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
			useMediaOverviewStore().$reset();
			useMediaStore().$reset();
			useNotificationsStore().$reset();
			useServerConnectionStore().$reset();
			useServerStore().$reset();
			useSettingsStore().$reset();
			useSignalrStore().$reset();
			useIntegrationStore().$reset();
		},
	};
	const getters = {
		getPageSetupReady: computed((): Observable<boolean> => state.pageReadyObservable.asObservable()),
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
