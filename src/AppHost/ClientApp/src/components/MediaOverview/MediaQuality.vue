<template>
	<!-- Show all available qualities as chips -->
	<div
		v-if="qualities.length"
		class="media-quality-container">
		<!-- Show all available qualities as chips -->
		<MediaVideoQuality
			v-for="(quality, j) in qualities"
			:key="j"
			:quality="quality.quality"
			:clickable="clickable"
			:truncated="truncated"
			:count="qualities.length"
			@click="$emit('download', [quality])" />
	</div>
</template>

<script setup lang="ts">
import type { PlexMediaQualityDTO } from '@dto';
import type { IMediaActionEmits } from '@interfaces';

withDefaults(defineProps<{
	qualities: PlexMediaQualityDTO[];
	truncated?: boolean; // Whether to truncate the display of qualities
	// Whether the chips are clickable
	clickable?: boolean;
}>(), {
	truncated: true,
	clickable: false,
});

defineEmits<IMediaActionEmits>();
</script>

<style lang="scss" scoped>
.media-quality-container {
  text-align: center;
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  justify-content: center;
}
</style>
