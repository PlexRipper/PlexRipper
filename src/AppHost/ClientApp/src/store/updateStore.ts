import { acceptHMRUpdate, defineStore } from 'pinia';
import { computed, reactive, toRefs } from 'vue';
import { of, type Observable, tap } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';
import Log from 'consola';
import { updateApi } from '@api';
import type { ReleaseNoteDTO } from '@api/generated/data-contracts';
import { RefreshDataType } from '@api/generated/data-contracts';
import { StoreNames, type ISetupResult } from '@interfaces';
import { cloneDeep } from 'lodash-es';
import { useSignalrStore } from '@store';

interface IUpdateStoreState {
	updateCheckError: string | null;
	hasUpdateAvailable: boolean;
	releaseNotes: ReleaseNoteDTO[];
}

export const useUpdateStore = defineStore(StoreNames.UpdateStore, () => {
	const defaultState: IUpdateStoreState = {
		updateCheckError: null,
		hasUpdateAvailable: false,
		releaseNotes: [],
	};

	const state = reactive<IUpdateStoreState>(cloneDeep(defaultState));
	const signalrStore = useSignalrStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			signalrStore
				.getRefreshNotification(RefreshDataType.UpdateAvailable)
				.subscribe(() => actions.checkForUpdate());

			return actions.checkForUpdate().pipe(switchMap(() => of({ name: StoreNames.UpdateStore, isSuccess: true })));
		},
		checkForUpdate() {
			return updateApi.checkForUpdateEndpoint().pipe(tap(({ isSuccess, value }) => {
				if (isSuccess && value) {
					state.hasUpdateAvailable = value.isUpdateAvailable;
					state.releaseNotes = value.releaseNotes;
				}
			}));
		},
		downloadUpdate() {
			return updateApi.downloadUpdateEndpoint();
		},
		applyUpdate() {
			return updateApi.applyUpdateEndpoint();
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {
		isUpdateAvailable: computed(() => state.hasUpdateAvailable),
		releaseNotes: computed(() => state.releaseNotes),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useUpdateStore, import.meta.hot));
}
