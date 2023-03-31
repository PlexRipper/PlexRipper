<template>
	<!-- The "Are you sure" dialog -->
	<q-col cols="12">
		<q-dialog :model-value="showDialog" :max-width="500">
			<q-card v-if="isConfirmationEnabled" bordered>
				<q-card-title> {{ $t('components.download-confirmation.header') }}</q-card-title>
				<q-card-subtitle class="py-2">
					<span>{{ $t('components.download-confirmation.description') }}</span> <br />
					<span>{{ $t('components.download-confirmation.total-size') }}<q-file-size :size="totalSize" /></span>
				</q-card-subtitle>
				<q-separator />
				<!-- Show Download Task Preview -->
				<q-card-section>
					<div style="min-height: 60vh; max-height: 60vh">
						{{ downloadPreview }}
						<q-tree :nodes="downloadPreview" label-key="title" node-key="id" />
					</div>
				</q-card-section>

				<q-separator />

				<q-card-actions>
					<CancelButton @click="showDialog = false" />
					<q-space />
					<ConfirmButton @click="confirmDownload()" />
				</q-card-actions>
			</q-card>
		</q-dialog>
	</q-col>
</template>

<script setup lang="ts">
import Log from 'consola';
import { computed, defineEmits, ref, defineExpose } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { SettingsService } from '@service';
import { toDownloadPreview } from '#imports';
import IDownloadPreview from '@interfaces/components/IDownloadPreview';

const emit = defineEmits<{
	(e: 'download', downloadCommand: DownloadMediaDTO[]): void;
}>();

const askDownloadMovieConfirmation = ref(false);
const askDownloadTvShowConfirmation = ref(false);
const askDownloadSeasonConfirmation = ref(false);
const askDownloadEpisodeConfirmation = ref(false);

const showDialog = ref(false);
const downloadPreview = ref<IDownloadPreview[]>([]);
const downloadMediaCommand = ref<DownloadMediaDTO[]>([]);

const isConfirmationEnabled = computed(() => {
	if (downloadMediaCommand.value.length === 1) {
		switch (downloadMediaCommand.value[0].type) {
			case PlexMediaType.Movie:
				return askDownloadMovieConfirmation.value;
			case PlexMediaType.TvShow:
				return askDownloadTvShowConfirmation.value;
			case PlexMediaType.Season:
				return askDownloadSeasonConfirmation.value;
			case PlexMediaType.Episode:
				return askDownloadEpisodeConfirmation.value;
			default:
				return true;
		}
	}
	return true;
});

const totalSize = computed(() => {
	let size = 0;
	if (downloadPreview.value.length > 0) {
		downloadPreview.value.forEach((x) => (size += x.size ?? 0));
	}
	return size;
});

const openDialog = (data: DownloadMediaDTO[], mediaItems: PlexMediaSlimDTO[]): void => {
	showDialog.value = true;
	downloadMediaCommand.value = data;
	downloadPreview.value = toDownloadPreview(data, mediaItems);
	// setDownloadMediaCommand(downloadMediaCommand);
	Log.info('downloadMedia', data);
	Log.info('mediaItems', mediaItems);
	Log.info('downloadPreview.value', downloadPreview.value);
	// if (isConfirmationEnabled.value) {
	// 	createPreview(downloadMediaCommand);
	// } else {
	// 	confirmDownload();
	// }
	// setShowDialog(true);
};

const confirmDownload = (): void => {
	emit('download', downloadMediaCommand.value);
	showDialog.value = false;
};

const closeDialog = (): void => {
	showDialog.value = false;
	downloadPreview.value = [];
};

onMounted(() => {
	useSubscription(
		SettingsService.getAskDownloadMovieConfirmation().subscribe((value) => {
			askDownloadMovieConfirmation.value = value;
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadTvShowConfirmation().subscribe((value) => {
			askDownloadTvShowConfirmation.value = value;
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadSeasonConfirmation().subscribe((value) => {
			askDownloadSeasonConfirmation.value = value;
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadEpisodeConfirmation().subscribe((value) => {
			askDownloadEpisodeConfirmation.value = value;
		}),
	);
});

defineExpose({
	openDialog,
});
</script>
