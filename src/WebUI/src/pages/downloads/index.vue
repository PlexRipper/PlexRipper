<template>
	<v-container fluid>
		<v-row>
			<v-col cols="auto">
				<h2>Downloads</h2>
				<!--				<p>{{ downloadProgressList }}</p>-->
				<!--				<p>{{ fileMergeProgressList }}</p>-->
				<!--				<p>{{ downloadStatusList }}</p>-->
			</v-col>
		</v-row>
		<!-- Download Toolbar -->
		<v-row>
			<v-col>
				<v-toolbar>
					<!--Prioritize buttons-->
					<v-btn-toggle borderless group tile :max="0">
						<v-btn>
							<v-icon large>mdi-arrow-collapse-up</v-icon>
						</v-btn>

						<v-btn>
							<v-icon large>mdi-arrow-up</v-icon>
						</v-btn>

						<v-btn>
							<v-icon large>mdi-arrow-down</v-icon>
						</v-btn>

						<v-btn>
							<v-icon large>mdi-arrow-collapse-down</v-icon>
						</v-btn>
					</v-btn-toggle>

					<v-spacer />

					<!--Command buttons-->
					<v-btn depressed tile @click="clearDownloadTasks">
						<v-icon large left>mdi-notification-clear-all</v-icon>
						<span class="hidden-sm-and-down">Clear Completed</span>
					</v-btn>
					<v-btn depressed tile>
						<v-icon large left>mdi-pause</v-icon>
						<span class="hidden-sm-and-down">Pause</span>
					</v-btn>

					<v-btn depressed tile>
						<v-icon large left>mdi-play</v-icon>
						<span class="hidden-sm-and-down">Start</span>
					</v-btn>

					<v-btn depressed tile>
						<v-icon large left>mdi-restart</v-icon>
						<span class="hidden-sm-and-down">Restart</span>
					</v-btn>

					<v-btn depressed tile @click="deleteDownloadTasks">
						<v-icon large left>mdi-delete</v-icon>
						<span class="hidden-sm-and-down">Delete</span>
					</v-btn>
				</v-toolbar>
			</v-col>
		</v-row>
		<!--	The Download Table	-->
		<v-row v-if="plexServers.length > 0">
			<v-col>
				<v-expansion-panels v-model="openExpansions" multiple>
					<v-expansion-panel v-for="plexServer in plexServers" :key="plexServer.id">
						<v-expansion-panel-header>
							<h2>{{ plexServer.name }}</h2>
						</v-expansion-panel-header>
						<v-expansion-panel-content>
							<downloads-table
								v-model="selected"
								:downloads="getDownloadRows(plexServer.id)"
								@pause="pauseDownloadTask"
								@delete="deleteDownloadTask"
								@stop="stopDownloadTask"
								@restart="restartDownloadTask"
								@start="startDownloadTask"
							/>
						</v-expansion-panel-content>
					</v-expansion-panel>
				</v-expansion-panels>
			</v-col>
		</v-row>
		<v-row v-else justify="center">
			<v-col cols="auto">
				<h2>There are currently no downloads in progress</h2>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import {
	deleteDownloadTask,
	restartDownloadTask,
	stopDownloadTask,
	clearDownloadTasks,
	startDownloadTask,
	pauseDownloadTask,
	deleteDownloadTasks,
} from '@api/plexDownloadApi';
import DownloadService from '@service/downloadService';
import SignalrService from '@service/signalrService';
import { finalize, switchMap } from 'rxjs/operators';
import {
	DownloadProgress,
	DownloadStatus,
	DownloadStatusChanged,
	DownloadTaskDTO,
	FileMergeProgress,
	PlexServerDTO,
} from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import DownloadsTable from './components/DownloadsTable.vue';
import IDownloadRow from './types/IDownloadRow';

@Component({
	components: {
		LoadingSpinner,
		DownloadsTable,
	},
})
export default class Downloads extends Vue {
	plexServers: PlexServerDTO[] = [];
	downloads: DownloadTaskDTO[] = [];
	downloadProgressList: DownloadProgress[] = [];
	fileMergeProgressList: FileMergeProgress[] = [];
	downloadStatusList: DownloadStatusChanged[] = [];
	openExpansions: number[] = [];
	selected: IDownloadRow[] = [];
	/**
	 * Merge the SignalR feeds into 1 update IDownloadRow
	 */
	getDownloadRows(serverId: number): IDownloadRow[] {
		const downloadRows: IDownloadRow[] = [];
		const downloads: DownloadTaskDTO[] = this.downloads.filter((x) => x.plexServerId === serverId);
		for (let i = 0; i < downloads.length; i++) {
			const download = downloads[i];
			const downloadProgress = this.downloadProgressList.find((x) => x.id === download.id);
			const downloadStatusUpdate = this.downloadStatusList.find((x) => x.id === download.id);
			// Merge the various feeds
			const downloadRow: IDownloadRow = {
				...download,
				...downloadProgress,
				// Status priority: downloadStatusUpdate > getDownloadList
				status: downloadStatusUpdate?.status ?? download.status,
			} as IDownloadRow;

			if (downloadRow.status === DownloadStatus.Merging) {
				const fileMergeProgress = this.fileMergeProgressList.find((x) => x.downloadTaskId === download.id);
				downloadRow.percentage = fileMergeProgress?.percentage ?? 0;
				downloadRow.dataReceived = fileMergeProgress?.dataTransferred ?? 0;
				downloadRow.timeRemaining = fileMergeProgress?.timeRemaining ?? 0;
				downloadRow.downloadSpeed = fileMergeProgress?.transferSpeed ?? 0;
			}

			if (downloadRow.status === DownloadStatus.Completed) {
				downloadRow.percentage = 100;
				downloadRow.timeRemaining = 0;
				downloadRow.downloadSpeed = 0;
				this.cleanupProgress(download.id);
			}
			downloadRows.push(downloadRow);
		}
		return downloadRows;
	}

	get selectedIds(): number[] {
		return this.selected.map((x) => x.id);
	}

	updateDownloadProgress(downloadProgress: DownloadProgress): void {
		// Check if there is already a progress object for this Id
		const i = this.downloadProgressList.findIndex((x) => x.id === downloadProgress.id);
		if (i > -1) {
			// Update
			this.downloadProgressList.splice(i, 1, downloadProgress);
		} else {
			// Add
			this.downloadProgressList.push(downloadProgress);
		}
	}

	deleteDownloadTask(downloadTaskId: number): void {
		const i = this.downloads.findIndex((x) => x.id === downloadTaskId);
		if (i > -1) {
			this.downloads.splice(i, 1);
		}
		deleteDownloadTask(downloadTaskId)
			.pipe(finalize(() => DownloadService.fetchDownloadList()))
			.subscribe(() => {
				this.cleanupProgress(downloadTaskId);
			});
	}

	stopDownloadTask(downloadTaskId: number): void {
		stopDownloadTask(downloadTaskId)
			.pipe(switchMap(() => DownloadService.fetchDownloadList))
			.subscribe();
	}

	pauseDownloadTask(downloadTaskId: number): void {
		pauseDownloadTask(downloadTaskId).subscribe();
	}

	startDownloadTask(downloadTaskId: number): void {
		startDownloadTask(downloadTaskId).subscribe();
	}

	restartDownloadTask(downloadTaskId: number): void {
		restartDownloadTask(downloadTaskId).subscribe();
	}

	clearDownloadTasks(): void {
		clearDownloadTasks()
			.pipe(switchMap(() => DownloadService.fetchDownloadList()))
			.subscribe();
	}

	deleteDownloadTasks(): void {
		deleteDownloadTasks(this.selected.map((x) => x.id))
			.pipe(switchMap(() => DownloadService.fetchDownloadList()))
			.subscribe();
	}

	cleanupProgress(downloadTaskId: number): void {
		// Clean-up progress objects
		const downloadProgressIndex = this.downloadProgressList.findIndex((x) => x.id === downloadTaskId);
		if (downloadProgressIndex > -1) {
			this.downloadProgressList.splice(downloadProgressIndex, 1);
		}

		// const downloadStatusUpdateIndex = this.downloadStatusList.findIndex((x) => x.id === downloadTaskId);
		// if (downloadStatusUpdateIndex > -1) {
		// 	this.downloadStatusList.splice(downloadStatusUpdateIndex, 1);
		// }

		const fileMergeProgressIndex = this.fileMergeProgressList.findIndex((x) => x.downloadTaskId === downloadTaskId);
		if (fileMergeProgressIndex > -1) {
			this.fileMergeProgressList.splice(fileMergeProgressIndex, 1);
		}
	}

	created(): void {
		DownloadService.getDownloadListInServers().subscribe((data) => {
			this.plexServers = data;
			this.openExpansions = [...Array(this.plexServers?.length).keys()] ?? [];
		});

		// Retrieve download list
		DownloadService.getDownloadList().subscribe((data) => {
			this.downloads = data ?? [];
		});

		// Retrieve download status from SignalR
		SignalrService.getDownloadStatus().subscribe((data) => {
			if (data) {
				const index = this.downloadStatusList.findIndex((x) => x.id === data.id);
				if (index > -1) {
					this.downloadStatusList.splice(index, 1);
					// Clean-up progress result if the download has finished
					if (data.status === DownloadStatus.Completed) {
						const progressIndex = this.downloadProgressList.findIndex((x) => x.id === data.id);
						if (progressIndex > -1) {
							this.downloadProgressList.splice(progressIndex, 1);
						}
					}
				}
				this.downloadStatusList.push(data);
			}
		});

		SignalrService.getDownloadProgress().subscribe((data) => {
			if (data) {
				this.updateDownloadProgress(data);
			} else {
				Log.error(`DownloadProgress was undefined`);
			}
		});

		SignalrService.getFileMergeProgress().subscribe((fileMergeProgress) => {
			if (fileMergeProgress) {
				// Check if there is already a progress object for this Id
				const i = this.fileMergeProgressList.findIndex((x) => x.id === fileMergeProgress.id);
				if (i > -1) {
					// Update
					this.fileMergeProgressList.splice(i, 1, fileMergeProgress);
				} else {
					// Add
					this.fileMergeProgressList.push(fileMergeProgress);
				}
			} else {
				Log.error(`FileMergeProgress was undefined`);
			}
		});
	}
}
</script>
<style scoped lang="scss">
tr.v-data-table__selected {
	background: transparent !important;
}
</style>
