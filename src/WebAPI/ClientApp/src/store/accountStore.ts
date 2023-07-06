import { acceptHMRUpdate, defineStore } from 'pinia';
import { forkJoin, Observable, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { PlexAccountDTO } from '@dto/mainApi';
import { createAccount, deleteAccount, getAllAccounts, refreshAccount, updateAccount } from '@api/accountApi';
import ISetupResult from '@interfaces/service/ISetupResult';

export const useAccountStore = defineStore('AccountStore', () => {
	const state = reactive<{ accounts: PlexAccountDTO[] }>({
		accounts: [],
	});

	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.refreshAccounts().pipe(switchMap(() => of({ name: useAccountStore.name, isSuccess: true })));
		},
		refreshAccounts() {
			return getAllAccounts().pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						state.accounts = result.value;
					}
				}),
			);
		},
		reSyncAccount(accountId: number) {
			return refreshAccount(accountId).pipe(
				switchMap(() => this.refreshAccounts()),
				switchMap(() => useServerStore().refreshPlexServers()),
			);
		},
		/**
		 * Creates a PlexAccount in the database, returns the new accountId and then also refreshes all the Plex Servers that are accessible
		 * @param {PlexAccountDTO} account
		 */
		createPlexAccount(account: PlexAccountDTO) {
			return createAccount(account).pipe(
				switchMap(() =>
					forkJoin([
						this.refreshAccounts(),
						useServerStore().refreshPlexServers(),
						useLibraryStore().refreshLibraries(),
					]),
				),
				switchMap(() => of(actions.getAccount(account.id))),
			);
		},
		updatePlexAccount(account: PlexAccountDTO, inspect = false) {
			return updateAccount(account, inspect).pipe(
				switchMap(() =>
					forkJoin([
						this.refreshAccounts(),
						useServerStore().refreshPlexServers(),
						useLibraryStore().refreshLibraries(),
					]),
				),
				switchMap(() => of(actions.getAccount(account.id))),
			);
		},
		deleteAccount(accountId: number) {
			return deleteAccount(accountId).pipe(
				switchMap(() =>
					forkJoin([
						this.refreshAccounts(),
						useServerStore().refreshPlexServers(),
						useLibraryStore().refreshLibraries(),
					]),
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