<template>
	<div
		v-if="qualities.length"
		class="media-quality-container">
		<!-- Show all available qualities as chips -->
		<QHover
			v-for="(quality, j) in qualities"
			:key="j">
			<template #default="{ }">
				<QGlowChip
					class="hover-expand-chip"
					clickable
					:color="getQualityDisplay(quality.quality).color"
					size="md"
					:value="qualities.length > minCount ? '' : getQualityDisplay(quality.quality).label"
					@click="$emit('download', [quality])" />
				<template v-if="qualities.length > minCount">
					<q-tooltip
						anchor="bottom middle"
						self="top middle"
						:offset="[0, 0]"
						class="no-background">
						<QGlowChip
							class="hover-expand-chip"
							:color="getQualityDisplay(quality.quality).color"
							size="md"
							:value="getQualityDisplay(quality.quality).label" />
					</q-tooltip>
				</template>
			</template>
		</QHover>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { VideoQuality, type PlexMediaQualityDTO } from '@dto';
import type { IMediaActionEmits } from '@interfaces';

const props = withDefaults(defineProps<{
	qualities: PlexMediaQualityDTO[];
	trunacted?: boolean; // Whether to truncate the display of qualities
}>(), {
	trunacted: true, // Default to false if not provided
});

defineEmits<IMediaActionEmits>();
const minCount = computed(() => {
	return props.trunacted ? 1 : 100;
});

const getQualityDisplay = (quality: VideoQuality): {
	color: string;
	label: string;
} => {
	switch (quality) {
		case VideoQuality.SubSD144P: // "144p"
			return {
				color: 'brown-6',
				label: '144p',
			};
		case VideoQuality.SubSDCIF: // "240p"
			return {
				color: 'deep-orange-6',
				label: '240p',
			};
		case VideoQuality.NHD: // "360p"
			return {
				color: 'orange-7',
				label: '360p',
			};
		case VideoQuality.SD: // "480p"
			return {
				color: 'amber-7',
				label: 'SD (480p)',
			};
		case VideoQuality.DVD: // "576p"
			return {
				color: 'yellow-7',
				label: 'DVD (576p)',
			};
		case VideoQuality.HD: // "720p"
			return {
				color: 'light-green-13',
				label: 'HD (720p)',
			};
		case VideoQuality.FullHD: // "1080p"
			return {
				color: 'light-blue-6',
				label: 'Full HD (1080p)',
			};
		case VideoQuality.QHD: // "1440p"
			return {
				color: 'cyan-6',
				label: 'QHD (1440p)',
			};
		case VideoQuality.UHD4K: // "2160p"
			return {
				color: 'red darken-4',
				label: '4K (2160p)',
			};
		case VideoQuality.UHD8K: // "4320p"
			return {
				color: 'purple-8',
				label: '8K (4320p)',
			};
		case VideoQuality.Unknown:
		default:
			Log.error('Missing quality display mapping for', quality);
			return {
				color: 'blue-grey-4',
				label: 'Unknown',
			};
	}
};
</script>

<style lang="scss">
.media-quality-container {
  text-align: center;
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  justify-content: center;
}
</style>
