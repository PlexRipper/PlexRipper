import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { forkJoin, of, Subject } from 'rxjs';
import { switchMap, take, tap } from 'rxjs/operators';
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

interface IAppConfigStoreState {
	version: string;
	config: IAppConfig;
	pageReadyObservable: Subject<boolean>;
}

export const useGlobalStore = defineStore('GlobalStore', () => {
	const defaultState: IAppConfigStoreState = {
		version: '?',
		config: {} as IAppConfig,
		pageReadyObservable: new Subject<boolean>(),
	};

	const state = reactive<IAppConfigStoreState>(cloneDeep(defaultState));

	const actions = {
		setupServices({ config }: { config: IAppConfig }): Observable<ISetupResult[]> {
			Log.info('Runtime Config is ready:', config);

			state.config = config;

			return actions.setup();
		},
		setup() {
			return of('').pipe(
				tap(() => state.pageReadyObservable.next(false)),
				switchMap(() =>
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
						useLocalizationStore().setup(),
						useMediaStore().setup(),
						useNotificationsStore().setup(),
						useServerConnectionStore().setup(),
						useServerStore().setup(),
						useSettingsStore().setup(),
						useSignalrStore().setup(),
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
			if (!version || state.version === version) {
				return;
			}
			Log.info('PlexRipper App Version:', version);
			state.version = version;
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
