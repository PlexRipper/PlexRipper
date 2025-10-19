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

	thumbWidth?: number;
	thumbHeight?: number;
}>(), {
	overlay: false,
	actions: false,
	allMediaMode: false,
	thumbWidth: 200,
	thumbHeight: 300,
});
const imageUrl = ref('');
const loading = ref(true);

defineEmits<IMediaActionEmits>();

onMounted(() => {
	if (!props.mediaItem?.hasThumb || props.mediaItem.metaDataKey === 0 || props.mediaItem.key === 0) {
		set(imageUrl, '');
		set(loading, false);
		return;
	}

	const useLowQualityPoster = settingsStore.generalSettings.useLowQualityPosterImages;
	useSubscription(mediaStore.getMediaThumbnailUrl({
		plexServerId: props.mediaItem.plexServerId,
		plexKey: props.mediaItem.key.toString(),
		metaDataKey: props.mediaItem.metaDataKey,
		width: useLowQualityPoster ? props.thumbWidth : props.thumbWidth * 1.5,
		height: useLowQualityPoster ? props.thumbHeight : props.thumbHeight * 1.5,
	}).subscribe((url) => {
		set(imageUrl, url);
		set(loading, false);
	}));
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
