import { defineStore, acceptHMRUpdate } from 'pinia';
import Log from 'consola';
import { map, switchMap, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { sum, merge, keyBy, values, flatMapDeep, clone } from 'lodash-es';
import {
	DownloadMediaDTO,
	DownloadPreviewDTO,
	DownloadProgressDTO,
	DownloadStatus,
	PlexServerDTO,
	ServerDownloadProgressDTO,
} from '@dto/mainApi';
import {
	clearDownloadTasks,
	deleteDownloadTasks,
	downloadMedia,
	getAllDownloads,
	pauseDownloadTask,
	postPreviewDownload,
	restartDownloadTasks,
	startDownloadTask,
	stopDownloadTasks,
} from '@api/plexDownloadApi';
import ISetupResult from '@interfaces/service/ISetupResult';
import { useServerStore } from '#build/imports';
import IDownloadsSelection from '@interfaces/IDownloadsSelection';
import IPTreeTableSelectionKeys from '@interfaces/IPTreeTableSelectionKeys';
export const useDownloadStore = defineStore('DownloadStore', () => {
	const state = reactive<{ serverDownloads: ServerDownloadProgressDTO[]; selected: IDownloadsSelection[] }>({
		serverDownloads: [],
		selected: [],
	});

	const serverStore = useServerStore();

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.fetchDownloadList().pipe(switchMap(() => of({ name: useDownloadStore.name, isSuccess: true })));
		},
		/**
		 * Fetch the download list from the API.
		 */
		fetchDownloadList() {
			return getAllDownloads().pipe(
				tap((downloads) => {
					if (downloads.isSuccess) {
						state.serverDownloads = downloads.value ?? [];
					}
				}),
			);
		},
		executeDownloadCommand(action: string, downloadTaskIds: number[]): void {
			const downloadTaskId = downloadTaskIds[0];
			// TODO verify if we need to re-fetch the download list after each action
			switch (action) {
				case 'pause':
					pauseDownloadTask(downloadTaskId).subscribe();
					break;
				case 'clear':
					clearDownloadTasks(downloadTaskIds).pipe(switchMap(actions.fetchDownloadList)).subscribe();
					break;
				case 'delete':
					deleteDownloadTasks(downloadTaskIds).pipe(switchMap(actions.fetchDownloadList)).subscribe();
					break;
				case 'stop':
					stopDownloadTasks(downloadTaskId).subscribe();
					break;
				case 'restart':
					restartDownloadTasks(downloadTaskId).subscribe();
					break;
				case 'start':
					startDownloadTask(downloadTaskId).subscribe();
					break;
				default:
					Log.error(`Action: ${action} does not have a assigned command with payload: ${downloadTaskIds}`);
			}
		},
		updateServerDownloadProgress(serverDownloadProgress: ServerDownloadProgressDTO): void {
			const i = state.serverDownloads.findIndex((x) => x.id === serverDownloadProgress.id);
			if (i === -1) {
				state.serverDownloads.push(serverDownloadProgress);
				actions.setupSelection(serverDownloadProgress.id);
				return;
			}

			const merged: DownloadProgressDTO[] = values(
				merge(keyBy(state.serverDownloads[i].downloads, 'id'), keyBy(serverDownloadProgress.downloads, 'id')),
			);

			state.serverDownloads.splice(i, 1, {
				...state.serverDownloads[i],
				downloads: merged,
			});
		},
		previewDownload(downloadMediaCommand: DownloadMediaDTO[]): Observable<DownloadPreviewDTO[]> {
			return postPreviewDownload(downloadMediaCommand).pipe(
				map((response) => {
					if (response && response.isSuccess) {
						return response.value ?? [];
					}
					return [];
				}),
			);
		},
		downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): void {
			downloadMedia(downloadMediaCommand)
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
					[`${v.mediaType}-${v.id}`]: {
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
			state.selected.splice(i, 1, {
				...state.selected[i],
				selection: value ? clone(state.selected[i].allSelection) : {},
			});
		},
		updateSelectedDownloadTasks(serverId: number, selection: IPTreeTableSelectionKeys): void {
			let i = state.selected.findIndex((x) => x.plexServerId === serverId);
			if (i === -1) {
				actions.setupSelection(serverId);
				i = state.selected.length - 1;
			}
			state.selected.splice(i, 1, {
				plexServerId: serverId,
				allSelection: state.selected[i].allSelection,
				maxSelectionCount: state.selected[i].maxSelectionCount,
				selection,
			});
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
			return getters
				.getDownloadsByServerId(serverId)
				.filter(
					(y) =>
						y.status === DownloadStatus.Downloading ||
						y.status === DownloadStatus.Moving ||
						y.status === DownloadStatus.Merging ||
						y.status === DownloadStatus.Paused,
				);
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
