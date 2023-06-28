import { defineStore, acceptHMRUpdate } from 'pinia';
import { Observable, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { PlexServerDTO } from '@dto/mainApi';
import { getPlexServer, getPlexServers } from '@api/plexServerApi';
import ISetupResult from '@interfaces/service/ISetupResult';

export const useServerStore = defineStore('ServerStore', {
	state: (): { servers: PlexServerDTO[] } => ({
		servers: [],
	}),
	actions: {
		setup(): Observable<ISetupResult> {
			return this.refreshPlexServers().pipe(switchMap(() => of({ name: useServerStore.name, isSuccess: true })));
		},
		refreshPlexServer(serverId: number) {
			return getPlexServer(serverId).pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						const i = this.servers.findIndex((x) => x.id === serverId);
						if (i > -1) {
							this.servers.splice(i, 1, result.value);
						}
					}
				}),
			);
		},
		/**
		 * Forces a refresh of all the PlexServers currently in store by fetching it from the API.
		 */
		refreshPlexServers() {
			return getPlexServers().pipe(
				tap((plexServers) => {
					if (plexServers.isSuccess) {
						this.servers = plexServers?.value ?? [];
					}
				}),
			);
		},
	},
	getters: {
		getServer:
			(state) =>
			(serverId: number): PlexServerDTO | null => {
				return state.servers.find((x) => x.id === serverId) ?? null;
			},
		getServers:
			(state) =>
			(serverIds: number[] = []): PlexServerDTO[] =>
				state.servers.filter((server) => !serverIds.length || serverIds.includes(server.id)),

		/**
		 * Retrieves the accessible PlexServers for the given PlexAccount from the store
		 * @param state
		 */
		getServersByPlexAccountId:
			(state) =>
			(plexAccountId: number): PlexServerDTO[] => {
				if (plexAccountId === 0) {
					return [];
				}
				const accountStore = useAccountStore();
				const account = accountStore.getAccount(plexAccountId);
				if (!account) {
					return [];
				}
				const serverIds = account.plexServerAccess.map((x) => x.plexServerId);
				return state.servers.filter((server) => serverIds.includes(server.id));
			},
		getServerStatus: () => (plexServerId: number) =>
			useServerConnectionStore()
				.getServerConnectionsByServerId(plexServerId)
				.some((x) => x.latestConnectionStatus?.isSuccessful ?? false),
	},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useServerStore, import.meta.hot));
}
