import { toRefs } from 'vue';
import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { forkJoin, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import type { CreatePlexAccountEndpointRequest, PlexAccountDTO } from '@dto';
import { RefreshDataType } from '@dto';
import type { ISetupResult } from '@interfaces';
import { plexAccountApi } from '@api';
import { useLibraryStore, useServerStore, useSignalrStore } from '@store';
import { cloneDeep } from 'lodash-es';

interface IAccountStoreState {
	accounts: PlexAccountDTO[];
	accessSyncLoading: boolean;
}

export const useAccountStore = defineStore('AccountStore', () => {
	const defaultState = {
		accessSyncLoading: false,
		accounts: [],
	};

	const state = reactive<IAccountStoreState>(cloneDeep(defaultState));

	const serverStore = useServerStore();
	const libraryStore = useLibraryStore();
	const signalRStore = useSignalrStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			// Listen for refresh notifications
			signalRStore.getRefreshNotification(RefreshDataType.PlexAccount).pipe(switchMap(() => actions.refreshAccounts())).subscribe();

			return actions.refreshAccounts().pipe(switchMap(() => of({ name: 'useAccountStore', isSuccess: true })));
		},
		refreshAccounts() {
			return plexAccountApi.getAllPlexAccountsEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						state.accounts = result.value;
					}
				}),
			);
		},
		reSyncAccount(accountId: number) {
			state.accessSyncLoading = true;
			return plexAccountApi.refreshPlexAccountAccessEndpoint(accountId).pipe(
				tap(() =>	forkJoin([actions.refreshAccounts(), serverStore.refreshPlexServers(), libraryStore.refreshLibraries()])),
				tap(() => state.accessSyncLoading = false),
			);
		},
		/**
     * Creates a PlexAccount in the database, returns the new accountId and then also refreshes all the Plex Servers that are accessible
     * @param {PlexAccountDTO} account
     */
		createPlexAccount(account: CreatePlexAccountEndpointRequest): Observable<void> {
			return plexAccountApi.createPlexAccountEndpoint(account).pipe(
				switchMap(() =>
					forkJoin([actions.refreshAccounts(), serverStore.refreshPlexServers(), libraryStore.refreshLibraries()]),
				),
				switchMap(() => of(void 0)),
			);
		},
		updatePlexAccount(account: PlexAccountDTO) {
			return plexAccountApi
				.updatePlexAccountByIdEndpoint(account)
				.pipe(
					switchMap(() =>
						forkJoin([actions.refreshAccounts(), serverStore.refreshPlexServers(), libraryStore.refreshLibraries()]),
					),
					switchMap(() => of(actions.getAccount(account.id))),
				);
		},
		deleteAccount(accountId: number) {
			return plexAccountApi.deletePlexAccountByIdEndpoint(accountId).pipe(switchMap(() => actions.refreshAccounts()));
		},
		getAccount(id: number): PlexAccountDTO | undefined {
			return state.accounts.find((x) => x.id === id);
		},
		/**
     * Checks if there is any account that has access to the server
     * NOTE: This will only check enabled accounts
     * @param plexServerId
     */
		getHasAccountServerAccess(plexServerId: number): boolean {
			for (const account of state.accounts.filter((x) => x.isEnabled)) {
				if (account.plexServerAccess.includes(plexServerId)) {
					return true;
				}
			}

			return false;
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	// Getters
	const getters = {
		getAccounts: computed(() => state.accounts),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useAccountStore, import.meta.hot));
}
