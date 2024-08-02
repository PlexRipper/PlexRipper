import { defineStore, acceptHMRUpdate } from 'pinia';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { switchMap, tap, map } from 'rxjs/operators';
import type { PlexServerDTO } from '@dto';
import type { ISetupResult } from '@interfaces';
import { plexServerApi } from '@api';
import { useAccountStore, useServerConnectionStore, useSettingsStore } from '#build/imports';

export const useServerStore = defineStore('ServerStore', () => {
	const state = reactive<{ servers: PlexServerDTO[] }>({
		servers: [],
	});
	const accountStore = useAccountStore();
	const serverConnectionStore = useServerConnectionStore();
	const settingsStore = useSettingsStore();
	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.refreshPlexServers().pipe(switchMap(() => of({ name: useServerStore.name, isSuccess: true })));
		},
		refreshPlexServer(serverId: number) {
			return plexServerApi.getPlexServerByIdEndpoint(serverId).pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						const i = state.servers.findIndex((x) => x.id === serverId);
						if (i > -1) {
							state.servers.splice(i, 1, result.value);
						}
					}
				}),
			);
		},
		/**
		 * Forces a refresh of all the PlexServers currently in store by fetching it from the API.
		 */
		refreshPlexServers(): Observable<PlexServerDTO[]> {
			return plexServerApi.getAllPlexServersEndpoint().pipe(
				tap((plexServers) => {
					if (plexServers.isSuccess) {
						state.servers = plexServers?.value ?? [];
					}
				}),
				map(() => state.servers),
			);
		},
		setServerAlias(serverId: number, serverAlias: string) {
			return plexServerApi
				.setServerAlias(serverId, {
					serverAlias,
				})
				.pipe(switchMap(() => settingsStore.refreshSettings()));
		},
		setServerHidden(serverId: number, hidden: boolean) {
			return plexServerApi
				.setServerHiddenRequestEndpoint(serverId, {
					hidden,
				})
				.pipe(switchMap(() => settingsStore.refreshSettings()));
		},
	};

	// Getters
	const getters = {
		getServer: (serverId: number): PlexServerDTO | null => {
			return state.servers.find((x) => x.id === serverId) ?? null;
		},
		getServers: (serverIds: number[] = []): PlexServerDTO[] =>
			state.servers.filter((server) => !serverIds.length || serverIds.includes(server.id)),
		getVisibleServers: computed((): PlexServerDTO[] =>
			getters.getServers().filter((x) => settingsStore.isServerVisible(x.machineIdentifier)),
		),
		getHiddenServers: computed((): PlexServerDTO[] =>
			getters.getServers().filter((x) => !settingsStore.isServerVisible(x.machineIdentifier)),
		),
		/**
		 * Retrieves the accessible PlexServers for the given PlexAccount from the store
		 */
		getServersByPlexAccountId: (plexAccountId: number): PlexServerDTO[] => {
			if (plexAccountId === 0) {
				return [];
			}
			const account = accountStore.getAccount(plexAccountId);
			if (!account) {
				return [];
			}
			const serverIds = account.plexServerAccess.map((x) => x.plexServerId);
			return state.servers.filter((server) => serverIds.includes(server.id));
		},
		getServerName: (serverId: number): string => {
			if (settingsStore.shouldMaskServerNames) {
				return '**MASKED**';
			}
			const server = getters.getServer(serverId);
			if (!server) {
				return '**UNKNOWN**';
			}

			const serverSettings = settingsStore.getServerSettings(server.machineIdentifier);
			if (serverSettings && serverSettings.plexServerName) {
				return serverSettings.plexServerName;
			}

			return server.name;
		},
		getServerStatus: (plexServerId: number) =>
			serverConnectionStore
				.getServerConnectionsByServerId(plexServerId)
				.some((x) => x.latestConnectionStatus?.isSuccessful ?? false),
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useServerStore, import.meta.hot));
}
