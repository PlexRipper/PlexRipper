import { acceptHMRUpdate, defineStore } from 'pinia';
import { forkJoin, Observable, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import type { PlexAccountDTO } from '@dto';
import type { ISetupResult } from '@interfaces';
import { useServerStore, useLibraryStore } from '#build/imports';
import { plexAccountApi } from '@api';

export const useAccountStore = defineStore('AccountStore', () => {
	const state = reactive<{ accounts: PlexAccountDTO[] }>({
		accounts: [],
	});

	const serverStore = useServerStore();
	const libraryStore = useLibraryStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.refreshAccounts().pipe(switchMap(() => of({ name: useAccountStore.name, isSuccess: true })));
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
			return plexAccountApi.refreshPlexAccountAccessEndpoint(accountId).pipe(
				switchMap(() => actions.refreshAccounts()),
				switchMap(() => serverStore.refreshPlexServers()),
			);
		},
		/**
		 * Creates a PlexAccount in the database, returns the new accountId and then also refreshes all the Plex Servers that are accessible
		 * @param {PlexAccountDTO} account
		 */
		createPlexAccount(account: PlexAccountDTO): Observable<PlexAccountDTO | undefined> {
			return plexAccountApi.createPlexAccountEndpoint(account).pipe(
				switchMap(() =>
					forkJoin([actions.refreshAccounts(), serverStore.refreshPlexServers(), libraryStore.refreshLibraries()]),
				),
				switchMap(() => of(actions.getAccount(account.id))),
			);
		},
		updatePlexAccount(account: PlexAccountDTO, inspect = false) {
			return plexAccountApi
				.updatePlexAccountByIdEndpoint(
					{
						inspect,
					},
					account,
				)
				.pipe(
					switchMap(() =>
						forkJoin([actions.refreshAccounts(), serverStore.refreshPlexServers(), libraryStore.refreshLibraries()]),
					),
					switchMap(() => of(actions.getAccount(account.id))),
				);
		},
		deleteAccount(accountId: number) {
			return plexAccountApi
				.deletePlexAccountByIdEndpoint(accountId)
				.pipe(
					switchMap(() =>
						forkJoin([actions.refreshAccounts(), serverStore.refreshPlexServers(), libraryStore.refreshLibraries()]),
					),
				);
		},
		getAccount(id: number): PlexAccountDTO | undefined {
			return state.accounts.find((x) => x.id === id);
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
