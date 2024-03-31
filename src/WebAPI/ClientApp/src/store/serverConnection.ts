import { acceptHMRUpdate, defineStore } from 'pinia';
import { Observable, of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { get } from '@vueuse/core';
import type { PlexServerConnectionDTO, PlexServerStatusDTO } from '@dto';
import { useServerStore } from '#build/imports';
import type { ISetupResult } from '@interfaces';
import { plexServerApi, plexServerConnectionApi } from '@api';

export const useServerConnectionStore = defineStore('ServerConnection', () => {
	const state = reactive<{ serverConnections: PlexServerConnectionDTO[] }>({
		serverConnections: [],
	});
	const actions = {
		setup(): Observable<ISetupResult> {
			return actions
				.refreshPlexServerConnections()
				.pipe(switchMap(() => of({ name: useServerConnectionStore.name, isSuccess: true })));
		},
		refreshPlexServerConnections(): Observable<PlexServerConnectionDTO[]> {
			return plexServerConnectionApi.getAllPlexServerConnectionsEndpoint().pipe(
				tap((serverConnections) => {
					if (serverConnections.isSuccess) {
						state.serverConnections = serverConnections.value ?? [];
					}
				}),
				map(() => get(getters.getServerConnections)),
			);
		},
		checkServerConnection(plexServerConnectionId: number): Observable<PlexServerStatusDTO | null> {
			return plexServerConnectionApi.checkConnectionStatusByIdEndpoint(plexServerConnectionId).pipe(
				map((serverStatus) => {
					if (serverStatus.isSuccess && serverStatus.value) {
						const index = state.serverConnections.findIndex((x) => x.id === plexServerConnectionId);
						if (index === -1) {
							return serverStatus.value;
						}
						state.serverConnections.splice(index, 1, {
							...state.serverConnections[index],
							serverStatusList: [serverStatus.value, ...state.serverConnections[index].serverStatusList],
							latestConnectionStatus: serverStatus.value,
						});
					}
					return serverStatus?.value ?? null;
				}),
			);
		},
		/**
		 * Forces a recheck of all the server connections for the given server id
		 * @param plexServerId
		 */
		checkServerStatus(plexServerId: number) {
			return plexServerConnectionApi.checkAllConnectionsStatusByPlexServerEndpoint(plexServerId).pipe(
				map((x) => x?.value ?? []),
				switchMap(() => actions.refreshPlexServerConnections()),
			);
		},
		setPreferredPlexServerConnection(serverId: number, connectionId: number) {
			return plexServerApi
				.setPreferredPlexServerConnectionEndpoint(serverId, connectionId)
				.pipe(switchMap(() => useServerStore().refreshPlexServer(serverId)));
		},
	};
	const getters = {
		getServerConnectionsByServerId: (plexServerId = 0): PlexServerConnectionDTO[] => {
			return state.serverConnections.filter((connection) =>
				plexServerId > 0 ? connection.plexServerId === plexServerId : false,
			);
		},
		getServerConnections: computed((): PlexServerConnectionDTO[] => state.serverConnections),
		isServerConnected: (plexServerId = 0) => {
			return state.serverConnections
				.filter((x) => x.plexServerId === plexServerId)
				.some((x) => x.latestConnectionStatus?.isSuccessful ?? false);
		},
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useServerConnectionStore, import.meta.hot));
}
