<template>
	<v-container>
		<v-row>
			<v-col>
				<p>Downloads</p>
				<downloads-table
					:downloads="getDownloadRows"
					@pause="pauseDownloadTask"
					@delete="deleteDownloadTasks"
					@stop="stopDownloadTask"
				/>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { deleteDownloadTask, stopDownloadTask } from '@api/plexDownloadApi';
import DownloadService from '@service/downloadService';
import SignalrService from '@service/signalrService';
import { tap, switchMap, finalize } from 'rxjs/operators';
import { DownloadTaskDTO, DownloadProgress } from '@dto/mainApi';
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
	downloads: DownloadTaskDTO[] = [];
	downloadProgressList: DownloadProgress[] = [];

	get getDownloadRows(): IDownloadRow[] {
		const downloadRows: IDownloadRow[] = [];
		for (let i = 0; i < this.downloads.length; i++) {
			downloadRows.push({
				...this.downloads[i],
				...this.downloadProgressList.find((x) => x.id === this.downloads[i].id),
			} as IDownloadRow);
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

	created(): void {
		// Retrieve download list and then retrieve the signalR download progress
		DownloadService.getDownloadList()
			.pipe(
				tap((data) => {
					this.downloads = data ?? [];
				}),
				switchMap(() => SignalrService.getDownloadProgress()),
			)
			.subscribe((data) => {
				if (data) {
					this.updateDownloadProgress(data);
				} else {
					Log.error(`DownloadProgress was undefined`);
				}
			});
	}
}
</script>
