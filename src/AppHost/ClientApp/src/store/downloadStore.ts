import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { map, switchMap, tap } from 'rxjs/operators';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { clone, cloneDeep, flatMapDeep, keyBy, merge, sum, values } from 'lodash-es';
import {
	type BaseResultDTO,
	type CreateDownloadTasksRequest,
	DownloadActions,
	type DownloadMediaDTO, type DownloadPreviewContainerDTO,
	type DownloadProgressDTO,
	DownloadStatus,
	type PlexServerDTO,
	type ServerDownloadProgressDTO,
} from '@dto';
import type { IDownloadsSelection, IPTreeTableSelectionKeys, ISetupResult } from '@interfaces';
import { downloadApi } from '@api';
import { useServerStore } from '@store';

interface IDownloadsStoreState {
	serverDownloads: ServerDownloadProgressDTO[];
	selected: IDownloadsSelection[];
}

export const useDownloadStore = defineStore('DownloadStore', () => {
	const defaultState: IDownloadsStoreState = {
		serverDownloads: [],
		selected: [],
	};

	const state = reactive<IDownloadsStoreState>(cloneDeep(defaultState));

	const serverStore = useServerStore();

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.fetchDownloadList().pipe(switchMap(() => of({ name: 'useDownloadStore', isSuccess: true })));
		},
		/**
     * Fetch the download list from the API.
     */
		fetchDownloadList() {
			return downloadApi.getAllDownloadTasksEndpoint().pipe(
				tap((downloads) => {
					if (downloads.isSuccess) {
						state.serverDownloads = downloads.value ?? [];
					}
				}),
			);
		},
		executeBatchDownloadCommand(action: DownloadActions) {
			const downloadTaskIds = state.selected.flatMap((x) => Object.keys(x.selection));
			return actions.executeDownloadCommand(action, downloadTaskIds);
		},
		executeDownloadCommand(action: DownloadActions, downloadTaskIds: string[]): Observable<BaseResultDTO> {
			if (downloadTaskIds.length === 0) {
				Log.error(`No downloadTaskIds provided for action: ${action}`);
				return of({
					errors: [],
					isSuccess: false,
					statusCode: 400,
					successes: [],
				} as BaseResultDTO);
			}

			const downloadTaskId = downloadTaskIds[0];
			// TODO verify if we need to re-fetch the download list after each action
			if (!downloadTaskId && action !== DownloadActions.Clear && action !== DownloadActions.Delete) {
				Log.error(`No downloadTaskId provided for action: ${action}`);
				return of({
					errors: [],
					isSuccess: false,
					statusCode: 400,
					successes: [],
				} as BaseResultDTO);
			}
			const id = downloadTaskId as string;
			switch (action) {
				case DownloadActions.Pause:
					return downloadApi.pauseDownloadTaskEndpoint(id);
				case DownloadActions.Clear:
					return downloadApi
						.clearCompletedDownloadTasksEndpoint(downloadTaskIds)
						.pipe(switchMap(actions.fetchDownloadList));
				case DownloadActions.Delete:
					return downloadApi
						.deleteDownloadTaskEndpoint(downloadTaskIds)
						.pipe(switchMap(actions.fetchDownloadList));
				case DownloadActions.Stop:
					return downloadApi.stopDownloadTaskEndpoint(id);
				case DownloadActions.Restart:
					return downloadApi.restartDownloadTaskEndpoint(id);
				case DownloadActions.Start:
					return downloadApi.startDownloadTaskEndpoint(id);
				default:
					Log.error(`Action: ${action} does not have a assigned command with payload: ${downloadTaskIds}`);
					return of();
			}
		},
		updateServerDownloadProgress(serverDownloadProgress: ServerDownloadProgressDTO | null): void {
			if (!serverDownloadProgress) {
				Log.warn('Received null server download progress update');
				return;
			}

			const i = state.serverDownloads.findIndex((x) => x.id === serverDownloadProgress.id);
			if (i === -1) {
				state.serverDownloads.push(serverDownloadProgress);
				actions.setupSelection(serverDownloadProgress.id);
				return;
			}

			const existing = state.serverDownloads[i]!;
			const merged: DownloadProgressDTO[] = values(
				merge(keyBy(existing.downloads ?? [], 'id'), keyBy(serverDownloadProgress.downloads ?? [], 'id')),
			);

			state.serverDownloads.splice(i, 1, {
				...existing,
				id: existing.id,
				downloadableTasksCount: existing.downloadableTasksCount,
				downloads: merged,
			});
		},
		previewDownload(downloadMediaCommand: DownloadMediaDTO[]): Observable<DownloadPreviewContainerDTO | null> {
			return downloadApi.getDownloadPreviewEndpoint(downloadMediaCommand).pipe(
				map((response) => {
					if (response && response.isSuccess) {
						return response.value ?? null;
					}
					return null;
				}),
			);
		},
		downloadMedia(request: CreateDownloadTasksRequest): void {
			downloadApi
				.createDownloadTasksEndpoint(request)
				.pipe(switchMap(() => actions.fetchDownloadList()))
				.subscribe();
		},
		setupSelection(serverId: number): void {
			const downloads = getters.getDownloadsByServerId(serverId);
			const getLeafs = (x) => {
				if (!x.children || !x.children.length) {
					return x;
				}
				return [x, flatMapDeep(x.children, getLeafs)];
			};

			const allSelection: IPTreeTableSelectionKeys = flatMapDeep(downloads, getLeafs).reduce(
				(a, v) => ({
					...a,
					[v.id]: {
						checked: true,
						partialChecked: false,
					},
				}),
				{},
			);
			state.selected.push({
				plexServerId: serverId,
				allSelection,
				maxSelectionCount: Object.keys(allSelection).length,
				selection: {},
			});
		},
		setAllSelectedDownloadTasks(serverId: number, value: boolean): void {
			let i = state.selected.findIndex((x) => x.plexServerId === serverId);
			if (i === -1) {
				actions.setupSelection(serverId);
				i = state.selected.length - 1;
			}
			const current = state.selected[i]!;
			state.selected.splice(i, 1, {
				...current,
				selection: value ? clone(current.allSelection) : {},
			});
		},
		updateSelectedDownloadTasks(serverId: number, selection: IPTreeTableSelectionKeys): void {
			let i = state.selected.findIndex((x) => x.plexServerId === serverId);
			if (i === -1) {
				actions.setupSelection(serverId);
				i = state.selected.length - 1;
			}
			const current = state.selected[i]!;
			state.selected.splice(i, 1, {
				plexServerId: serverId,
				allSelection: current.allSelection,
				maxSelectionCount: current.maxSelectionCount,
				selection,
			});
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	// Getters
	const getters = {
		getDownloadsByServerId: (serverId = 0): DownloadProgressDTO[] => {
			if (serverId === 0) {
				return state.serverDownloads.flatMap((x) => x.downloads);
			}
			return state.serverDownloads.find((server) => server.id === serverId)?.downloads ?? [];
		},
		getServersWithDownloads: computed((): { plexServer: PlexServerDTO; downloads: DownloadProgressDTO[] }[] => {
			const serverIds = state.serverDownloads.map((x) => x.id);
			const plexServersWithDownloads = serverStore.servers.filter((x) => serverIds.includes(x.id));

			return plexServersWithDownloads.map((x) => {
				return {
					plexServer: x,
					downloads: getters.getDownloadsByServerId(x.id),
				};
			});
		}),
		getActiveDownloadList(serverId = 0): DownloadProgressDTO[] {
			return getters.getDownloadsByServerId(serverId).flatMap((x) => x.children).flatMap((x) => x.children).flatMap((x) => x.children).filter((x) => x.status != DownloadStatus.Completed && x.status != DownloadStatus.Error);
		},
		/**
     * Get the total number of download tasks that are downloadable in the download list.
     */
		getTotalDownloadsCount: computed((): number => {
			return sum(state.serverDownloads.flatMap((x) => x.downloadableTasksCount));
		}),
		hasSelected: computed(() => state.selected.some((x) => Object.keys(x?.selection ?? {}).length > 0)),
		getSelectedDownloadTasks(serverId: number): IPTreeTableSelectionKeys {
			return getters.getDownloadSelection(serverId)?.selection ?? {};
		},
		getDownloadSelection(serverId: number): IDownloadsSelection | null {
			return state.selected.find((x) => x.plexServerId === serverId) ?? null;
		},
		getHeaderSelection(serverId: number): boolean | null {
			const downloadSelection = getters.getDownloadSelection(serverId);
			const selectionLength = Object.keys(downloadSelection?.selection ?? {}).length;
			if (selectionLength === 0) {
				return false;
			}

			return downloadSelection?.maxSelectionCount === selectionLength ? true : null;
		},
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useDownloadStore, import.meta.hot));
}
