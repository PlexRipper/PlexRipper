import { defineStore, acceptHMRUpdate } from 'pinia';
import Log from 'consola';
import { map, switchMap, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { sum, merge, keyBy, values } from 'lodash-es';
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
import ISelection from '@interfaces/ISelection';
import ISetupResult from '@interfaces/service/ISetupResult';
import { useServerStore } from '#build/imports';
export const useDownloadStore = defineStore('DownloadStore', () => {
	const state = reactive<{ serverDownloads: ServerDownloadProgressDTO[]; selected: ISelection[] }>({
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
				tap((downloads) => {
					for (const server of downloads.value ?? []) {
						if (state.selected.some((x) => x.indexKey === server.id)) {
							continue;
						}
						state.selected.push({
							keys: [],
							indexKey: server.id,
							allSelected: false,
						});
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
					clearDownloadTasks(downloadTaskIds).subscribe();
					break;
				case 'delete':
					deleteDownloadTasks(downloadTaskIds).subscribe();
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
		clearDownloadTasks(downloadTaskIds: number[] = []): void {
			clearDownloadTasks(downloadTaskIds).subscribe(() => {
				actions.fetchDownloadList();
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
		updateSelected(id: number, payload: ISelection): void {
			const i = state.selected.findIndex((x) => x.indexKey === id);
			if (i === -1) {
				state.selected.push({ indexKey: id, keys: payload.keys, allSelected: payload.allSelected });
				return;
			}
			state.selected.splice(i, 1, {
				...state.selected[i],
				keys: payload.keys,
				allSelected: payload.allSelected,
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
		hasSelected: computed(() => state.selected.some((x) => x.keys.length > 0)),
		getSelected: (serverId: number): ISelection => state.selected.find((x) => x.indexKey === serverId) as ISelection,
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
