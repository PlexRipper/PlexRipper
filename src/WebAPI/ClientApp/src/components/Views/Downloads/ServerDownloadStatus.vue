<template>
	<v-col class="py-1">
		<v-btn depressed tile class="no-background" @click.native.stop="changeStatus">
			<v-icon large left>{{ getButtonIcon }}</v-icon>
			<span class="hidden-sm-and-down">{{ getButtonText }}</span>
		</v-btn>
	</v-col>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { DownloadStatus } from '@dto/mainApi';

@Component<ServerDownloadStatus>({})
export default class ServerDownloadStatus extends Vue {
	serverStatus: DownloadStatus = DownloadStatus.Paused;
	get getButtonIcon(): string {
		if (this.serverStatus === DownloadStatus.Paused) {
			return 'mdi-play';
		}

		if (this.serverStatus === DownloadStatus.Downloading) {
			return 'mdi-pause';
		}
		return 'mdi-question';
	}

	get getButtonText(): string {
		if (this.serverStatus === DownloadStatus.Paused) {
			return 'Start';
		}

		if (this.serverStatus === DownloadStatus.Downloading) {
			return 'Pause';
		}
		return 'mdi-question';
	}

	changeStatus(): void {
		if (this.serverStatus === DownloadStatus.Paused) {
			this.serverStatus = DownloadStatus.Downloading;
		} else if (this.serverStatus === DownloadStatus.Downloading) {
			this.serverStatus = DownloadStatus.Paused;
		}
	}
}
</script>
