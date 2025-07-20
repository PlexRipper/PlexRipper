<template>
	<q-card
		square
		flat
		:class="{ 'media-poster--fallback': fallback }"
		:style="{ height: thumbHeight + 'px', width: thumbWidth + 'px' }">
		<q-card-section
			v-if="fallback">
			<QRow justify="center">
				<QCol cols="auto">
					<QMediaTypeIcon
						:size="60"
						:media-type="mediaType" />
				</QCol>
			</QRow>
		</q-card-section>

		<q-card-section :class="{ 'q-py-none': fallback }">
			<QText
				:value="mediaItem.title"
				bold="bold"
				align="center"
				size="h6" />
			<QText
				v-if="mediaItem.type === PlexMediaType.TvShow"
				:value="$t('components.media-poster-image-content.seasons-count', { count: mediaItem.childCount }) "
				bold="bold"
				align="center"
				size="subtitle1" />
			<QText
				v-if="mediaItem.type === PlexMediaType.TvShow"
				:value="$t('components.media-poster-image-content.episode-count', { count: mediaItem.grandChildCount }) "
				bold="bold"
				align="center"
				size="subtitle1" />
			<QText
				v-if="allMediaMode"
				align="center"
				:size="allMediaMode ? 'subtitle2' : 'h6'"
				:value="serverStore.getServerName(mediaItem.plexServerId)" />
		</q-card-section>

		<QCardActions
			v-if="actions"
			align="center"
			class="media-poster--actions">
			<QRow :justify="mediaType === PlexMediaType.TvShow ? 'around' : 'center'">
				<QCol cols="auto">
					<BaseButton
						icon="mdi-download"
						size="xl"
						flat
						:outline="false"
						@click="$emit('download', [])" />
				</QCol>
				<QCol cols="auto">
					<BaseButton
						v-if="mediaType === PlexMediaType.TvShow"
						icon="mdi-magnify"
						:outline="false"
						size="xl"
						flat
						@click="$emit('open-media-details')" />
				</QCol>
			</QRow>
		</QCardActions>
	</q-card>
</template>

<script setup lang="ts">
import { type PlexMediaQualityDTO, type PlexMediaSlimDTO, PlexMediaType } from '@dto';
import { useServerStore } from '@store';
import type { IMediaActionEmits } from '@interfaces';

const serverStore = useServerStore();

const props = withDefaults(defineProps<{
	mediaItem: PlexMediaSlimDTO;
	fallback?: boolean;
	allMediaMode?: boolean;
	actions?: boolean;
	thumbWidth?: number;
	thumbHeight?: number;
}>(), {
	fallback: false,
	allMediaMode: false,
	actions: true,
	thumbWidth: 200,
	thumbHeight: 300,
});

defineEmits<IMediaActionEmits>();

const mediaType = computed(() => props.mediaItem?.type ?? PlexMediaType.Unknown);
</script>

<style lang="scss">
@use '@/assets/scss/_mixins.scss';

.media-poster {
  &--fallback {
    @extend .background-sm;
    padding: 0 !important;
  }

  &--actions {
    position: absolute;
    text-align: center;
    bottom: 0;
    left: 0;
    right: 0;
  }
}
</style>
