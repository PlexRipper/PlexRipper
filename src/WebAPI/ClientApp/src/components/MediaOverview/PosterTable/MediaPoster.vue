<template>
	<q-card flat>
		<q-card-section>
			<MediaPosterImage
				:media-item="mediaItem"
				:all-media-mode="mediaOverviewStore.allMediaMode"
				overlay
				actions
				@action="onAction" />
			<!--	Quality bar	-->
			<MediaQuality
				class="media-poster-quality-bar"
				:qualities="mediaItem.qualities" />
		</q-card-section>
		<QLoadingOverlay :loading="loading" />
	</q-card>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get } from '@vueuse/core';
import { type DownloadMediaDTO, type PlexMediaSlimDTO, PlexMediaType } from '@dto';
import { useMediaOverviewStore } from '@store';

const mediaOverviewStore = useMediaOverviewStore();

const props = defineProps<{
	mediaItem: PlexMediaSlimDTO;
}>();

const emit = defineEmits<{
	(e: 'download', downloadMediaCommands: DownloadMediaDTO[]): void;
	(e: 'open-media-details', payload: PlexMediaSlimDTO): void;
}>();

const loading = ref(false);
const mediaType = computed(() => props.mediaItem?.type ?? PlexMediaType.Unknown);

function onAction(event: 'download' | 'open-media-details') {
	const downloadCommand: DownloadMediaDTO = {
		type: get(mediaType),
		mediaIds: [props.mediaItem.id],
		plexLibraryId: props.mediaItem.plexLibraryId,
		plexServerId: props.mediaItem.plexServerId,
	};

	switch (event) {
		case 'download':
			emit('download', [downloadCommand]);
			break;
		case 'open-media-details':
			emit('open-media-details', props.mediaItem);
			break;
		default:
			Log.error('Unknown action event', event);
			break;
	}
}
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.media-poster-quality-bar {
  @extend .background-sm;
}
</style>
