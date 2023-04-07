<template>
	<q-col class="q-py-sm">
		<q-btn tile class="no-background" :icon="getButtonIcon" @click.stop="changeStatus">
			<span>{{ getButtonText }}</span>
		</q-btn>
	</q-col>
</template>

<script setup lang="ts">
import { DownloadStatus } from '@dto/mainApi';

const serverStatus = ref<DownloadStatus>(DownloadStatus.Paused);

const getButtonIcon = computed(() => {
	if (serverStatus.value === DownloadStatus.Paused) {
		return 'mdi-play';
	}

	if (serverStatus.value === DownloadStatus.Downloading) {
		return 'mdi-pause';
	}

	return 'mdi-question';
});

const getButtonText = computed(() => {
	if (serverStatus.value === DownloadStatus.Paused) {
		return 'Start';
	}

	if (serverStatus.value === DownloadStatus.Downloading) {
		return 'Pause';
	}
	return 'mdi-question';
});

function changeStatus(): void {
	if (serverStatus.value === DownloadStatus.Paused) {
		serverStatus.value = DownloadStatus.Downloading;
	} else if (serverStatus.value === DownloadStatus.Downloading) {
		serverStatus.value = DownloadStatus.Paused;
	}
}
</script>
