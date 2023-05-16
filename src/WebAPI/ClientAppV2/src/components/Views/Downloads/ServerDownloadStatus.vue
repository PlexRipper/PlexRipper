<template>
	<q-col class="q-py-sm">
		<BaseButton flat :icon="getButtonIcon" :label="$t(getButtonText)" @click.stop="changeStatus" />
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
		return 'components.server-download-status.start';
	}

	if (serverStatus.value === DownloadStatus.Downloading) {
		return 'components.server-download-status.pause';
	}
	return '';
});

function changeStatus(): void {
	if (serverStatus.value === DownloadStatus.Paused) {
		serverStatus.value = DownloadStatus.Downloading;
	} else if (serverStatus.value === DownloadStatus.Downloading) {
		serverStatus.value = DownloadStatus.Paused;
	}
}
</script>
