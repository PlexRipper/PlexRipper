<template>
	<v-container>
		<v-row>
			<v-col>
				<p>Downloads</p>
				<p>{{ downloadStatusList }}</p>
				<p>{{ downloadProgressList }}</p>
			</v-col>
		</v-row>
		<v-row>
			<v-expansion-panels v-model="openExpansions" multiple :dark="$vuetify.theme.dark">
				<v-expansion-panel v-for="plexServer in plexServers" :key="plexServer.id">
					<v-expansion-panel-header>{{ plexServer.name }}</v-expansion-panel-header>
					<v-expansion-panel-content>
						<downloads-table
							:downloads="getDownloadRows"
							@pause="pauseDownloadTask"
							@delete="deleteDownloadTasks"
							@stop="stopDownloadTask"
							@restart="restartDownloadTask"
						/>
					</v-expansion-panel-content>
				</v-expansion-panel>
			</v-expansion-panels>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { deleteDownloadTask, stopDownloadTask, restartDownloadTask } from '@api/plexDownloadApi';
import DownloadService from '@service/downloadService';
import SignalrService from '@service/signalrService';
import { finalize } from 'rxjs/operators';
import { DownloadTaskDTO, DownloadProgress, PlexServerDTO, DownloadStatusChanged } from '@dto/mainApi';
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
	downloadStatusList: DownloadStatusChanged[] = [];
	openExpansions: number[] = [];

	/**
	 * Merge the SignalR feeds into 1 update IDownloadRow
	 */
	get getDownloadRows(): IDownloadRow[] {
		const downloadRows: IDownloadRow[] = [];
		for (let i = 0; i < this.downloads.length; i++) {
			const download = this.downloads[i];
			const downloadProgress = this.downloadProgressList.find((x) => x.id === download.id);
			const downloadStatusUpdate = this.downloadStatusList.find((x) => x.id === download.id);
			// Merge the various feeds
			const downloadRow: IDownloadRow = {
				...download,
				...downloadProgress,
				// Status priority: downloadProgress > downloadStatusUpdate > getDownloadList
				status: downloadStatusUpdate?.status ?? download.status,
			} as IDownloadRow;

			downloadRows.push(downloadRow);
		}
		return downloadRows;
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
		deleteDownloadTask(downloadTaskId).subscribe(() => {
			// Refresh downloadTasks
			DownloadService.fetchDownloadList();
		});
	}

	stopDownloadTask(downloadTaskId: number): void {
		stopDownloadTask(downloadTaskId)
			.pipe(finalize(() => DownloadService.fetchDownloadList))
			.subscribe();
	}

	pauseDownloadTask(downloadTaskId: number): void {
		Log.debug(`Pausing download task with id ${downloadTaskId}, which is doing nothing as of now`);
	}

	startDownloadTask(downloadTaskId: number): void {
		Log.debug(`Starting download task with id ${downloadTaskId}, which is doing nothing as of now`);
	}

	restartDownloadTask(downloadTaskId: number): void {
		restartDownloadTask(downloadTaskId).subscribe();
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
			const index = this.downloadStatusList.findIndex((x) => x.id === data.id);
			if (index > -1) {
				this.downloadStatusList.splice(index, 1);
			}
			this.downloadStatusList.push(data);
		});

		SignalrService.getDownloadProgress().subscribe((data) => {
			if (data) {
				this.updateDownloadProgress(data);
			} else {
				Log.error(`DownloadProgress was undefined`);
			}
		});
	}
}
</script>
