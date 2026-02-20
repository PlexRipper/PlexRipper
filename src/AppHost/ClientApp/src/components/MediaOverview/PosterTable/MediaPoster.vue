<template>
	<q-card flat>
		<q-card-section>
			<div class="media-poster-image-wrapper">
				<MediaPosterImage
					:media-item="mediaItem"
					:active="active"
					:all-media-mode="mediaOverviewStore.allMediaMode"
					overlay
					actions
					@download="onDownload"
					@open-media-details="$emit('open-media-details', mediaItem)" />
				<!--	Sort value overlay	-->
				<div
					v-if="mediaOverviewStore.getIsSorted"
					class="media-poster-sort-overlay">
					<QFileSize
						v-if="mediaOverviewStore.getActiveSort.field === MediaSortField.MediaSize"
						:size="mediaItem.mediaSize"
						align="center" />
					<QDuration
						v-else-if="mediaOverviewStore.getActiveSort.field === MediaSortField.Duration"
						:value="mediaItem.duration"
						align="center"
						short />
					<QDateTime
						v-else-if="mediaOverviewStore.getActiveSort.field === MediaSortField.AddedAt"
						:text="mediaItem.addedAt"
						align="center"
						short-date />
					<QDateTime
						v-else-if="mediaOverviewStore.getActiveSort.field === MediaSortField.UpdatedAt"
						:text="mediaItem.updatedAt ?? ''"
						align="center"
						short-date />
					<QText
						v-else-if="mediaOverviewStore.getActiveSort.field === MediaSortField.Year"
						:value="mediaItem.year"
						align="center" />
				</div>
			</div>
			<!--	Quality bar	-->
			<MediaQuality
				class="media-poster-quality-bar"
				:qualities="mediaItem.qualities"
				clickable
				@download="onDownload" />
		</q-card-section>
		<QLoadingOverlay :loading="loading" />
	</q-card>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { type DownloadMediaDTO, type PlexMediaQualityDTO, type PlexMediaSlimDTO, PlexMediaType } from '@dto';
import { MediaSortField } from '@enums';
import { useMediaOverviewStore } from '@store';

const mediaOverviewStore = useMediaOverviewStore();

const props = withDefaults(defineProps<{
	mediaItem: PlexMediaSlimDTO;
	active?: boolean;
}>(), {
	active: true,
});

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

.media-poster-image-wrapper {
  position: relative;
  display: block;
  width: 100%;
}

.media-poster-sort-overlay {
  @extend .background-lg;

  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 4px 8px;
  text-align: center;
  color: #fff;
  pointer-events: none;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
