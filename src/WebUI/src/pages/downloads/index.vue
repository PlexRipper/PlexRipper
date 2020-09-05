<template>
	<v-container fluid>
		<v-row>
			<v-col>
				<p>Downloads</p>
			</v-col>
		</v-row>
		<!-- Download Toolbar -->
		<v-row>
			<v-col>
				<v-toolbar>
					<!--Prioritize buttons-->
					<v-btn-toggle borderless group tile :dark="$vuetify.theme.dark">
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
					<v-btn-toggle borderless group tile :dark="$vuetify.theme.dark">
						<v-btn value="justify" @click="clearDownloadTasks">
							<v-icon large left>mdi-notification-clear-all</v-icon>
							<span class="hidden-sm-and-down">Clear Completed</span>
						</v-btn>
						<v-btn value="left">
							<v-icon large left>mdi-pause</v-icon>
							<span class="hidden-sm-and-down">Pause</span>
						</v-btn>

						<v-btn value="center">
							<v-icon large left>mdi-play</v-icon>
							<span class="hidden-sm-and-down">Resume</span>
						</v-btn>

						<v-btn value="right">
							<v-icon large left>mdi-delete</v-icon>
							<span class="hidden-sm-and-down">Delete</span>
						</v-btn>
					</v-btn-toggle>
				</v-toolbar>
			</v-col>
		</v-row>
		<!--	The Download Table	-->
		<v-row>
			<v-col>
				<v-expansion-panels v-model="openExpansions" multiple :dark="$vuetify.theme.dark">
					<v-expansion-panel v-for="plexServer in plexServers" :key="plexServer.id">
						<v-expansion-panel-header>
							<h2>{{ plexServer.name }}</h2>
						</v-expansion-panel-header>
						<v-expansion-panel-content>
							<downloads-table
								v-model="selected"
								:downloads="getDownloadRows(plexServer.id)"
								@pause="pauseDownloadTask"
								@delete="deleteDownloadTasks"
								@stop="stopDownloadTask"
								@restart="restartDownloadTask"
								@start="startDownloadTask"
							/>
						</v-expansion-panel-content>
					</v-expansion-panel>
				</v-expansion-panels>
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
		for (let i = 0; i < this.downloads.length; i++) {
			const download = this.downloads[i];
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
			}
			downloadRows.push(downloadRow);
		}
		return downloadRows.filter((x) => x.plexServerId === serverId);
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

	deleteDownloadTasks(downloadTaskId: number): void {
		deleteDownloadTask(downloadTaskId)
			.pipe(finalize(() => DownloadService.fetchDownloadList))
			.subscribe(() => {
				// Clean-up progress objects
				const downloadProgressIndex = this.downloadProgressList.findIndex((x) => x.id === downloadTaskId);
				this.downloadProgressList.splice(downloadProgressIndex, 1);

				const downloadStatusUpdateIndex = this.downloadStatusList.findIndex((x) => x.id === downloadTaskId);
				this.downloadStatusList.splice(downloadStatusUpdateIndex, 1);

				const fileMergeProgressIndex = this.fileMergeProgressList.findIndex((x) => x.downloadTaskId === downloadTaskId);
				this.fileMergeProgressList.splice(fileMergeProgressIndex, 1);
			});
	}

	stopDownloadTask(downloadTaskId: number): void {
		stopDownloadTask(downloadTaskId)
			.pipe(finalize(() => DownloadService.fetchDownloadList))
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
