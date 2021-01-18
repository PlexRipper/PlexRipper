<template>
	<page>
		<!-- Download Toolbar -->
		<download-bar
			:has-selected="hasSelected"
			@pause="pauseDownloadTasks"
			@stop="stopDownloadTasks"
			@restart="restartDownloadTasks"
			@start="startDownloadTasks"
			@clear="clearDownloadTasks"
			@delete="deleteDownloadTasks"
		/>
		<!--	The Download Table	-->
		<perfect-scrollbar class="download-page-tables">
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
									@clear="clearDownloadTask"
									@delete="deleteDownloadTask"
									@stop="stopDownloadTask"
									@restart="restartDownloadTask"
									@start="startDownloadTask"
									@details="detailsDownloadTask"
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
		</perfect-scrollbar>
		<download-details-dialog :download-task="downloadTaskDetail" :dialog="dialog" @close="closeDetailsDialog" />
	</page>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { pauseDownloadTask, restartDownloadTask, startDownloadTask } from '@api/plexDownloadApi';
import DownloadService from '@state/downloadService';
import SignalrService from '@service/signalrService';
import {
	DownloadProgress,
	DownloadStatus,
	DownloadStatusChanged,
	DownloadTaskDTO,
	FileMergeProgress,
	PlexServerDTO,
} from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import _ from 'lodash';
import DownloadsTable from './components/DownloadsTable.vue';
import IDownloadRow from './types/IDownloadRow';
import DownloadBar from '~/pages/downloads/components/DownloadBar.vue';
import DownloadDetailsDialog from '~/pages/downloads/components/DownloadDetailsDialog.vue';

@Component({
	components: {
		LoadingSpinner,
		DownloadsTable,
		DownloadBar,
		DownloadDetailsDialog,
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
	downloadTaskDetail: DownloadTaskDTO | null = null;
	private dialog: boolean = false;

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
				downloadRow.dataReceived = downloadRow.dataTotal;
				this.cleanupProgress(download.id);
			}
			downloadRows.push(downloadRow);
		}
		return downloadRows;
	}

	get selectedIds(): number[] {
		return this.selected.map((x) => x.id);
	}

	get hasSelected(): boolean {
		return this.selectedIds.length > 0;
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

	// region single commands

	clearDownloadTask(downloadTaskId: number): void {
		DownloadService.clearDownloadTasks([downloadTaskId]);
		this.selected = _.filter(this.selected, (x) => x.id !== downloadTaskId);
	}

	startDownloadTask(downloadTaskId: number): void {
		startDownloadTask(downloadTaskId).subscribe();
	}

	pauseDownloadTask(downloadTaskId: number): void {
		pauseDownloadTask(downloadTaskId).subscribe();
	}

	stopDownloadTask(downloadTaskId: number): void {
		DownloadService.stopDownloadTasks([downloadTaskId]);
	}

	restartDownloadTask(downloadTaskId: number): void {
		restartDownloadTask(downloadTaskId).subscribe();
	}

	deleteDownloadTask(downloadTaskId: number): void {
		DownloadService.deleteDownloadTasks([downloadTaskId]);
		this.selected = _.filter(this.selected, (x) => x.id !== downloadTaskId);
	}

	detailsDownloadTask(downloadTaskId: number): void {
		this.downloadTaskDetail = this.downloads.find((x) => x.id === downloadTaskId) ?? null;
		this.dialog = true;
	}

	// endregion

	// region batch commands
	clearDownloadTasks(): void {
		if (!this.hasSelected) {
			DownloadService.clearDownloadTasks([]);
		} else {
			DownloadService.clearDownloadTasks(this.selectedIds);
			this.selected = [];
		}
	}

	startDownloadTasks(): void {
		Log.info('startDownloadTasks not implemented');
	}

	pauseDownloadTasks(): void {
		Log.info('pauseDownloadTasks not implemented');
	}

	stopDownloadTasks(): void {
		Log.info('stopDownloadTasks not implemented');
	}

	restartDownloadTasks(): void {
		Log.info('restartDownloadTasks not implemented');
	}

	deleteDownloadTasks(): void {
		DownloadService.deleteDownloadTasks(this.selectedIds);
		this.selected = [];
	}

	// endregion

	closeDetailsDialog(): void {
		this.downloadTaskDetail = null;
		this.dialog = false;
	}

	cleanupProgress(downloadTaskId: number): void {
		// Clean-up progress objects
		const downloadProgressIndex = this.downloadProgressList.findIndex((x) => x.id === downloadTaskId);
		if (downloadProgressIndex > -1) {
			this.downloadProgressList.splice(downloadProgressIndex, 1);
		}

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
			Log.info('getDownloadList', data);
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
