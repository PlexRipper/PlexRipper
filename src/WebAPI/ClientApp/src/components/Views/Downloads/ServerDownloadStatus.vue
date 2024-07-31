<template>
	<QCol class="q-py-sm">
		<BaseButton
			flat
			:icon="getButtonIcon"
			:label="getButtonText"
			@click.stop="changeStatus" />
	</QCol>
</template>

<script setup lang="ts">
import { DownloadStatus } from '@dto';

const { t } = useI18n();
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
		return t('components.server-download-status.start');
	}

	if (serverStatus.value === DownloadStatus.Downloading) {
		return t('components.server-download-status.pause');
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
