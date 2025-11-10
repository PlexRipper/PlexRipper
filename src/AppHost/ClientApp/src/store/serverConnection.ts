import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { get } from '@vueuse/core';
import type {
	CreatePlexServerConnectionEndpointRequest,
	PlexServerConnectionDTO,
	PlexServerStatusDTO,
	UpdatePlexServerConnectionEndpointRequest,
} from '@dto';
import type { ISetupResult } from '@interfaces';
import { plexServerApi, plexServerConnectionApi } from '@api';
import { RefreshDataType } from '@dto';
import { sortPlexServerConnections } from '@composables/common';
import { useServerStore, useSignalrStore } from '@store';
import { assign, cloneDeep } from 'lodash-es';

interface IServerConnectionStoreState {
	serverConnections: PlexServerConnectionDTO[];
}

export const useServerConnectionStore = defineStore('ServerConnection', () => {
	const defaultState: IServerConnectionStoreState = {
		serverConnections: [],
	};

	const state = reactive<IServerConnectionStoreState>(cloneDeep(defaultState));

	const signalRStore = useSignalrStore();
	const serverStore = useServerStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			// Listen for refresh notifications
			signalRStore
				.getRefreshNotification(RefreshDataType.PlexServerConnection)
				.pipe(switchMap(() => actions.refreshPlexServerConnections()))
				.subscribe();

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
				map((res) => {
					if (res.isSuccess && res.value) {
						const index = state.serverConnections.findIndex((x) => x.id === plexServerConnectionId);
						if (index === -1) {
							return res.value;
						}
						assign(state.serverConnections[index]!.latestConnectionStatus, res.value);
					}
					return res?.value ?? null;
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
		checkServerConnectionUrl: (connectionUrl: string) =>
			plexServerConnectionApi.validatePlexServerConnectionEndpoint({
				url: connectionUrl,
			}),
		createServerConnection: (data: CreatePlexServerConnectionEndpointRequest) =>
			plexServerConnectionApi.createPlexServerConnectionEndpoint(data).pipe(
				tap(({ isSuccess, value }) => {
					if (isSuccess && value) {
						state.serverConnections.push(value);
					}
				}),
			),
		updateServerConnection: (data: UpdatePlexServerConnectionEndpointRequest) =>
			plexServerConnectionApi.updatePlexServerConnectionEndpoint(data).pipe(
				tap(({ isSuccess, value }) => {
					if (isSuccess && value) {
						const index = state.serverConnections.findIndex((x) => x.id === value.id);
						if (index !== -1 && value) {
							state.serverConnections.splice(index, 1, value);
						}
					}
				}),
			),
		deleteServerConnection: (connectionId: number) =>
			plexServerConnectionApi.deletePlexServerConnectionById(connectionId).pipe(
				tap(({ isSuccess }) => {
					if (isSuccess) {
						const index = state.serverConnections.findIndex((x) => x.id === connectionId);
						if (index !== -1) {
							state.serverConnections.splice(index, 1);
						}
					}
				}),
			),
		chooseServerConnection: (plexServerId: number): PlexServerConnectionDTO | null =>
			state.serverConnections.find(
				(x) => x.plexServerId === plexServerId && x.chosenConnection,
			) ?? null,
		setPreferredPlexServerConnection: (plexServerId: number, connectionId: number) =>
			plexServerApi
				.setPreferredPlexServerConnectionEndpoint(plexServerId, connectionId)
				.pipe(switchMap(() => serverStore.refreshPlexServer(plexServerId))),
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};
	const getters = {
		getServerConnectionsByServerId: (plexServerId = 0): PlexServerConnectionDTO[] =>
			sortPlexServerConnections(
				state.serverConnections.filter((connection) => (plexServerId > 0 ? connection.plexServerId === plexServerId : false)),
			),
		getServerConnection: (connectionId: number) => state.serverConnections.find((x) => x.id === connectionId),
		getServerConnections: computed((): PlexServerConnectionDTO[] => state.serverConnections),
		isServerConnected: (plexServerId = 0) =>
			state.serverConnections
				.filter((x) => x.plexServerId === plexServerId)
				.some((x) => x.latestConnectionStatus?.isSuccessful ?? false),
		IsUrlExisting: (
			url: string,
		): {
			plexServerId: number;
			connectionId: number;
		} => {
			const connection = state.serverConnections.find((x) => x.url === url);
			return connection
				? { plexServerId: connection.plexServerId, connectionId: connection.id }
				: {
						plexServerId: 0,
						connectionId: 0,
					};
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
