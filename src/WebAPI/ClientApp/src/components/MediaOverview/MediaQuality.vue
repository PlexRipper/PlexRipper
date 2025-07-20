<template>
	<div
		v-if="qualities.length"
		class="media-quality-container">
		<QGlowChip
			v-for="(quality, j) in qualities"
			:key="j"
			:color="getQualityDisplay(quality.quality).color"
			size="md"
			:value="getQualityDisplay(quality.quality).label" />
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { VideoQuality, type PlexMediaQualityDTO } from '@dto';

defineProps<{
	qualities: PlexMediaQualityDTO[];
}>();

const getQualityDisplay = (quality: VideoQuality): {
	color: string;
	label: string;
} => {
	switch (quality) {
		case VideoQuality.SD: // "480p"
			return {
				color: 'deep-orange',
				label: 'SD (480p)',
			};
		case VideoQuality.DVD: // "576p"
			return {
				color: 'yellow darken-1',
				label: 'DVD (576p)',
			};
		case VideoQuality.HD: // "720p"
			return {
				color: 'lime accent-4',
				label: 'HD (720p)',
			};
		case VideoQuality.FullHD: // "1080p"
			return {
				color: 'blue accent-3',
				label: 'Full HD (1080p)',
			};
		case VideoQuality.QHD: // "1440p"
			return {
				color: 'indigo accent-2',
				label: 'QHD (1440p)',
			};
		case VideoQuality.UHD4K: // "4K"
			return {
				color: 'red darken-4',
				label: '4K (2160p)',
			};
		case VideoQuality.UHD8K: // "8K"
			return {
				color: 'pink darken-4',
				label: '8K (4320p)',
			};
		case VideoQuality.Unknown:
		default:
			Log.error('Missing quality display mapping for', quality);
			return {
				color: 'blue-grey',
				label: 'Unknown',
			};
	}
};
</script>

<style lang="scss">
.media-quality-container {
  padding: 0;
  text-align: center;
}
</style>
