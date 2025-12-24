<template>
	<QGlowContainer>
		<q-skeleton
			v-if="loading"
			class="media-poster-skeleton"
			animation="fade"
			square
			dark />
		<template v-else>
			<QHover
				v-if="imageUrl"
				class="media-poster">
				<template #default="{ hover }">
					<q-img
						:key="mediaItem.id"
						loading="eager"
						:src="imageUrl"
						fit="fill"
						no-spinner
						crossorigin="anonymous"
						class="media-poster--image"
						:alt="mediaItem.title">
						<template #default>
							<!--	Overlay	-->
							<div :class="['media-poster--overlay', hover && overlay ? 'on-hover' : '', 'white--text']">
								<MediaPosterImageContent
									:media-item="mediaItem"
									:actions="actions"
									:all-media-mode="allMediaMode"
									@download="$emit('download', $event)"
									@open-media-details="$emit('open-media-details')" />
							</div>
						</template>
						<template #error>
							<!--	Show fallback image	-->
							<MediaPosterImageContent
								fallback
								:actions="actions"
								:media-item="mediaItem"
								:all-media-mode="allMediaMode"
								@download="$emit('download', $event)"
								@open-media-details="$emit('open-media-details')" />
						</template>
					</q-img>
				</template>
			</QHover>
			<!--	Show fallback image	-->
			<MediaPosterImageContent
				v-else
				fallback
				:actions="actions"
				:media-item="mediaItem"
				:all-media-mode="allMediaMode"
				@download="$emit('download', $event)"
				@open-media-details="$emit('open-media-details')" />
		</template>
	</QGlowContainer>
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import type { Subscription } from 'rxjs';
import type { PlexMediaSlimDTO } from '@dto';
import type { IMediaActionEmits } from '@interfaces';
import { useSettingsStore, useMediaStore } from '#imports';

const settingsStore = useSettingsStore();
const mediaStore = useMediaStore();

const props = withDefaults(defineProps<{
	mediaItem: PlexMediaSlimDTO;
	overlay?: boolean;
	actions?: boolean;
	allMediaMode?: boolean;
	active?: boolean;

	thumbWidth?: number;
	thumbHeight?: number;
}>(), {
	overlay: false,
	actions: false,
	allMediaMode: false,
	active: true,
	thumbWidth: 200,
	thumbHeight: 300,
});
const imageUrl = ref('');
const loading = ref(true);
let currentSubscription: Subscription | null = null;

defineEmits<IMediaActionEmits>();

function loadThumbnail(mediaItem: PlexMediaSlimDTO) {
	// Cancel any pending subscription when media item changes
	if (currentSubscription) {
		currentSubscription.unsubscribe();
		currentSubscription = null;
	}

	// Reset state for the new item
	set(loading, true);

	if (!mediaItem?.hasThumb || mediaItem.metaDataKey === 0 || mediaItem.key === 0) {
		set(imageUrl, '');
		set(loading, false);
		return;
	}

	const useLowQualityPoster = settingsStore.generalSettings.useLowQualityPosterImages;
	currentSubscription = mediaStore.getMediaThumbnailUrl({
		plexServerId: mediaItem.plexServerId,
		plexKey: mediaItem.key.toString(),
		metaDataKey: mediaItem.metaDataKey,
		width: useLowQualityPoster ? props.thumbWidth : props.thumbWidth * 1.5,
		height: useLowQualityPoster ? props.thumbHeight : props.thumbHeight * 1.5,
	}).subscribe((url) => {
		set(imageUrl, url);
		set(loading, false);
	});
}

// Watch for changes in mediaItem or active state (handles RecycleScroller element recycling)
// Only load thumbnail when the view is active to skip work on off-screen items
watch([() => props.mediaItem.id, () => props.active], ([, isActive]) => {
	if (isActive) {
		loadThumbnail(props.mediaItem);
	}
}, { immediate: true });

onUnmounted(() => {
	if (currentSubscription) {
		currentSubscription.unsubscribe();
	}
});
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.q-img__content > div {
  padding: 0;
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

    &.on-hover {
      opacity: 0.8;

      .q-btn {
        opacity: 1;
      }
    }
  }

}
</style>
