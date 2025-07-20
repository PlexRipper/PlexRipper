<template>
	<q-card flat>
		<q-card-section>
			<MediaPosterImage
				:media-item="mediaItem"
				:all-media-mode="mediaOverviewStore.allMediaMode"
				overlay
				actions
				@download="onDownload"
				@open-media-details="$emit('open-media-details', mediaItem)" />
			<!--	Quality bar	-->
			<MediaQuality
				class="media-poster-quality-bar"
				:qualities="mediaItem.qualities"
				@download="onDownload" />
		</q-card-section>
		<QLoadingOverlay :loading="loading" />
	</q-card>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { type DownloadMediaDTO, type PlexMediaQualityDTO, type PlexMediaSlimDTO, PlexMediaType } from '@dto';
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

function onDownload(mediaQualities: PlexMediaQualityDTO[]) {
	const downloadCommand: DownloadMediaDTO = {
		type: get(mediaType),
		mediaIds: [props.mediaItem.id],
		plexLibraryId: props.mediaItem.plexLibraryId,
		plexServerId: props.mediaItem.plexServerId,
		qualities: mediaQualities,
	};

	emit('download', [downloadCommand]);
}
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.media-poster-quality-bar {
  @extend .background-sm;
}
</style>
