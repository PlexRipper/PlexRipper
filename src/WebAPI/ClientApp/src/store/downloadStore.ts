import { defineStore, acceptHMRUpdate } from 'pinia';
import Log from 'consola';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { sum } from 'lodash-es';
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
import { SignalrService } from '@service';
export const useDownloadStore = defineStore('DownloadStore', {
	state: (): { serverDownloads: ServerDownloadProgressDTO[]; selected: ISelection[] } => ({
		serverDownloads: [],
		selected: [],
	}),
	actions: {
		setup(): void {
			this.fetchDownloadList();
			for (const server of this.serverDownloads) {
				if (this.selected.some((x) => x.indexKey === server.id)) {
					continue;
				}
				this.selected.push({
					keys: [],
					indexKey: server.id,
					allSelected: false,
				});
			}
			SignalrService.GetServerDownloadProgress().subscribe((data: ServerDownloadProgressDTO) => {
				Log.info('DownloadStore: SignalR: GetServerDownloadProgress', data);
				// this.serverDownloads = data;
			});
		},
		/**
		 * Fetch the download list from the API.
		 */
		fetchDownloadList(): void {
			getAllDownloads().subscribe((downloads) => {
				if (downloads.isSuccess) {
					this.serverDownloads = downloads.value ?? [];
				}
			});
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
		updateServerDownloadProgress(downloadProgress: DownloadProgressDTO): void {
			const i = this.serverDownloads.findIndex((x) => x.id === downloadProgress.id);
			if (i === -1) {
			}
		},
		clearDownloadTasks(downloadTaskIds: number[] = []): void {
			clearDownloadTasks(downloadTaskIds).subscribe(() => {
				this.fetchDownloadList();
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
			downloadMedia(downloadMediaCommand).subscribe(() => {
				this.fetchDownloadList();
			});
		},
		updateSelected(id: number, payload: ISelection): void {
			const i = this.selected.findIndex((x) => x.indexKey === id);
			if (i === -1) {
				this.selected.push({ indexKey: id, keys: payload.keys, allSelected: payload.allSelected });
				return;
			}
			this.selected.splice(i, 1, {
				...this.selected[i],
				keys: payload.keys,
				allSelected: payload.allSelected,
			});
		},
	},
	getters: {
		getDownloadsByServerId:
			(state) =>
			(serverId = 0): DownloadProgressDTO[] => {
				if (serverId === 0) {
					return state.serverDownloads.flatMap((x) => x.downloads);
				}
				return state.serverDownloads.find((server) => server.id === serverId)?.downloads ?? [];
			},
		getServersWithDownloads(): { plexServer: PlexServerDTO; downloads: DownloadProgressDTO[] }[] {
			const serverIds = this.serverDownloads.map((x) => x.id);
			const plexServersWithDownloads = plexServers.filter((x) => serverIds.includes(x.id));

			return plexServersWithDownloads.map((x) => {
				return {
					plexServer: x,
					downloads: this.getDownloadsByServerId(x.id),
				};
			});
		},
		getActiveDownloadList() {
			return (serverId = 0): DownloadProgressDTO[] => {
				if (!this) {
					Log.error('this is undefined in DownloadStore => getActiveDownloadList');
					return [];
				}
				return this.getDownloadsByServerId(serverId).filter(
					(y) =>
						y.status === DownloadStatus.Downloading ||
						y.status === DownloadStatus.Moving ||
						y.status === DownloadStatus.Merging ||
						y.status === DownloadStatus.Paused,
				);
			};
		},
		/**
		 * Get the total number of download tasks that are downloadable in the download list.
		 */
		getTotalDownloadsCount: (state): number => {
			return sum(state.serverDownloads.flatMap((x) => x.downloadableTasksCount));
		},
		hasSelected: (state) => state.selected.some((x) => x.keys.length > 0),
		getSelected:
			(state) =>
			(serverId: number): ISelection =>
				state.selected.find((x) => x.indexKey === serverId) as ISelection,
	},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useDownloadStore, import.meta.hot));
}
