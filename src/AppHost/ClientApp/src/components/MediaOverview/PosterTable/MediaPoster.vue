<template>
	<q-card
		class="media-poster-card"
		flat>
		<q-card-section>
			<div class="media-poster-image-wrapper">
				<QGlowContainer>
					<q-skeleton
						v-if="thumbnailLoading"
						class="media-poster-skeleton"
						animation="fade"
						square
						dark />
					<q-img
						v-else-if="imageUrl"
						loading="lazy"
						:src="imageUrl"
						fit="fill"
						no-spinner
						crossorigin="anonymous"
						class="media-poster--image"
						:alt="mediaItem.title">
						<template #default>
							<div class="media-poster--overlay white--text">
								<div class="media-poster--content">
									<div class="media-poster--section">
										<QText
											:value="mediaItem.title"
											bold="bold"
											align="center"
											size="h6"
											class="media-poster--title" />
										<QText
											v-if="mediaType === PlexMediaType.TvShow"
											:value="$t('components.media-poster-image-content.seasons-count', { count: mediaItem.childCount })"
											bold="bold"
											align="center"
											size="subtitle1" />
										<QText
											v-if="mediaType === PlexMediaType.TvShow"
											:value="$t('components.media-poster-image-content.episode-count', { count: mediaItem.grandChildCount })"
											bold="bold"
											align="center"
											size="subtitle1" />
										<QText
											v-if="mediaOverviewStore.allMediaMode"
											align="center"
											size="subtitle2"
											:value="serverStore.getServerName(mediaItem.plexServerId)" />
									</div>
									<div
										:class="['media-poster--actions', mediaType === PlexMediaType.TvShow ? 'media-poster--actions-around' : 'media-poster--actions-center']">
										<BaseButton
											icon="mdi-download"
											size="xl"
											flat
											:outline="false"
											@click="onDownload([])" />
										<BaseButton
											v-if="mediaType === PlexMediaType.TvShow"
											icon="mdi-magnify"
											:outline="false"
											size="xl"
											flat
											@click="emit('open-media-details', mediaItem)" />
									</div>
								</div>
							</div>
						</template>
					</q-img>
					<div
						v-else
						class="media-poster--content media-poster--fallback">
						<div class="media-poster--fallback-icon">
							<QMediaTypeIcon
								:size="60"
								:media-type="mediaType" />
						</div>
						<div class="media-poster--section media-poster--section-compact">
							<QText
								:value="mediaItem.title"
								bold="bold"
								align="center"
								size="h6"
								class="media-poster--title" />
						</div>
					</div>
				</QGlowContainer>

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
import { get, set } from '@vueuse/core';
import type { Subscription } from 'rxjs';
import { type DownloadMediaDTO, type PlexMediaQualityDTO, type PlexMediaSlimDTO, PlexMediaType } from '@dto';
import { MediaSortField } from '@enums';
import { useMediaOverviewStore, useMediaStore, useServerStore, useSettingsStore } from '@store';

const mediaOverviewStore = useMediaOverviewStore();
const mediaStore = useMediaStore();
const serverStore = useServerStore();
const settingsStore = useSettingsStore();

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
const thumbnailLoading = ref(true);
const imageUrl = ref('');
const thumbWidth = 200;
const thumbHeight = 300;
let currentSubscription: Subscription | null = null;

const mediaType = computed(() => props.mediaItem?.type ?? PlexMediaType.Unknown);

function onDownload(mediaQualities: PlexMediaQualityDTO[]) {
	const downloadCommand: DownloadMediaDTO = {
		type: get(mediaType),
		mediaIds: [props.mediaItem.id],
		plexLibraryId: props.mediaItem.plexLibraryId,
		plexServerId: props.mediaItem.plexServerId,
		qualities: mediaQualities,
		keepCompletedInDownloadFolder: settingsStore.downloadManagerSettings.keepCompletedInDownloadFolder,
	};

	emit('download', [downloadCommand]);
}

function loadThumbnail(mediaItem: PlexMediaSlimDTO) {
	if (currentSubscription) {
		currentSubscription.unsubscribe();
		currentSubscription = null;
	}

	set(thumbnailLoading, true);

	if (!mediaItem?.hasThumb || mediaItem.plexApiMetaDataKey === 0 || mediaItem.plexApiRatingKey === 0) {
		set(imageUrl, '');
		set(thumbnailLoading, false);
		return;
	}

	const useLowQualityPoster = settingsStore.generalSettings.useLowQualityPosterImages;
	currentSubscription = mediaStore.getMediaThumbnailUrl({
		plexServerId: mediaItem.plexServerId,
		plexKey: mediaItem.plexApiRatingKey.toString(),
		metaDataKey: mediaItem.plexApiMetaDataKey,
		width: useLowQualityPoster ? thumbWidth : thumbWidth * 1.5,
		height: useLowQualityPoster ? thumbHeight : thumbHeight * 1.5,
	}).subscribe({
		next: (url) => {
			set(imageUrl, url);
			set(thumbnailLoading, false);
		},
		error: () => {
			set(imageUrl, '');
			set(thumbnailLoading, false);
		},
	});
}

watch([() => props.mediaItem.id, () => props.active], ([, isActive]) => {
	if (isActive) {
		loadThumbnail(props.mediaItem);
		mediaOverviewStore.setCurrentScrollIndex(props.mediaItem.sortIndex);
	}
}, { immediate: true });

onUnmounted(() => {
	if (currentSubscription)
		currentSubscription.unsubscribe();
});
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.q-img__content > div {
  padding: 0;
}

.media-poster-card {
  flex: 0 0 232px !important;
  width: 232px !important;
  min-width: 232px !important;
  max-width: 232px !important;
}

.media-poster-skeleton {
  @extend .background-sm;
  width: 200px;
  height: 300px;
  padding: 0;
}

.media-poster {
  @extend .background-sm;

  width: 200px;

  &--image {
    height: 300px;
    padding: 0;
  }

  &--overlay {
    @extend .background-xl;
    width: 100%;
    height: 100%;
    opacity: 0;
    margin: 0;
    transition: opacity 0.2s ease-in-out;
  }

  &--content {
    overflow: hidden;
    width: 200px;
    height: 300px;
  }

  &--section {
    padding: 16px;
  }

  &--section-compact {
    padding-top: 0;
    padding-bottom: 0;
  }

  &--fallback {
    @extend .background-sm;
    padding: 0 !important;
  }

  &--fallback-icon {
    display: flex;
    justify-content: center;
    padding: 16px;
  }

  &--title {
    .q-text {
      display: -webkit-box;
      -webkit-line-clamp: 4;
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      word-break: break-word;
      overflow-wrap: break-word;
    }
  }

  &--actions {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    display: flex;
    align-items: center;
    width: 100%;
    padding: 8px;
    text-align: center;
  }

  &--actions-center {
    justify-content: center;
  }

  &--actions-around {
    justify-content: space-around;
  }
}

.media-poster-card:hover {
  .media-poster--overlay {
    opacity: 0.8;

    .q-btn {
      opacity: 1;
    }
  }
}

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
