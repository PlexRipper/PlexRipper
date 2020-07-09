<template>
	<v-row>
		<v-col>
			<p>Downloads</p>
			<v-btn @click="submitCard">Test button</v-btn>
			<downloads-table :downloads="downloads" />
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Log from 'consola';
import * as PlexDownloadApi from '@api/plexDownloadApi';
import IDownloadTask from '@dto/IDownloadTask';
import IDownloadProgress from '@dto/IDownloadProgress';
import DownloadsTable from './components/DownloadsTable.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { SignalrStore } from '@/store/';

@Component({
	components: {
		LoadingSpinner,
		DownloadsTable,
	},
})
export default class Downloads extends Vue {
	downloads: IDownloadTask[] = [];

	submitCard() {
		const progressConnection = SignalrStore.getDownloadProgressConnection;

		progressConnection.invoke('SendMessageAsync', 'this.userName', 'this.userMessage').catch(function(err) {
			Log.error(err.toSting());
		});
	}

	async created(): Promise<void> {
		this.downloads = await PlexDownloadApi.getAllDownloads();

		addEventListener('signalRSetup', () => {
			const progressConnection = SignalrStore.getDownloadProgressConnection;
			progressConnection.on('DownloadProgress', function(downloadProgress: IDownloadProgress) {
				Log.debug(downloadProgress);
			});
		});
	}
}
</script>
