<template>
	<v-container>
		<v-row>
			<v-col>
				<p>Downloads</p>
				<downloads-table :downloads="getDownloadRows" @delete="deleteDownloadTasks" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Log from 'consola';
import * as PlexDownloadApi from '@api/plexDownloadApi';
import IDownloadTask from '@dto/IDownloadTask';
import IDownloadProgress from '@dto/IDownloadProgress';
import DownloadsTable from './components/DownloadsTable.vue';
import IDownloadRow from './types/IDownloadRow';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { SignalrStore } from '@/store/';
import { deleteDownloadTask } from '@/types/api/plexDownloadApi';

@Component({
	components: {
		LoadingSpinner,
		DownloadsTable,
	},
})
export default class Downloads extends Vue {
	downloads: IDownloadTask[] = [];
	downloadProgressList: IDownloadProgress[] = [];

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

	updateDownloadProgress(downloadProgress: IDownloadProgress): void {
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

	async deleteDownloadTasks(downloadTaskId: number): Promise<void> {
		await deleteDownloadTask(downloadTaskId);
		// Refresh downloadTasks
		await this.getAllDownloadTasks();
	}

	async getAllDownloadTasks(): Promise<void> {
		this.downloads = await PlexDownloadApi.getAllDownloads();
	}

	async mounted(): Promise<void> {
		await this.getAllDownloadTasks();

		addEventListener('signalRSetup', () => {
			const progressConnection = SignalrStore.getDownloadProgressConnection;
			progressConnection.on('DownloadProgress', this.updateDownloadProgress);

			progressConnection.on('ReceiveMessage', function(user, message) {
				Log.debug(user);
				Log.debug(message);
			});
		});
	}
}
</script>
