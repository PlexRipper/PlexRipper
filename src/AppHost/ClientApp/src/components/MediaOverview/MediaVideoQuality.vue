<template>
	<QHover>
		<template #default="{ }">
			<QGlowChip
				class="hover-expand-chip"
				:clickable="clickable"
				:color="getQualityDisplay(quality).color"
				size="md"
				:value="count > minCount ? '' : getQualityDisplay(quality).label" />
			<template v-if="count > minCount">
				<q-tooltip
					anchor="bottom middle"
					self="top middle"
					:offset="[0, 0]"
					class="no-background">
					<QGlowChip
						class="hover-expand-chip"
						:color="getQualityDisplay(quality).color"
						size="md"
						has-background
						:value="getQualityDisplay(quality).label" />
				</q-tooltip>
			</template>
		</template>
	</QHover>
</template>

<script setup lang="ts">
import Log from 'consola';
import { VideoQuality } from '@dto';
import type { IMediaActionEmits } from '@interfaces';
import { translateVideoQuality } from '@composables';

const props = withDefaults(defineProps<{
	quality: VideoQuality;
	truncated?: boolean; // Whether to truncate the display of qualities
	// Whether the chips are clickable
	clickable?: boolean;
	count?: number;
}>(), {
	truncated: true,
	clickable: false,
	count: 1,
});

defineEmits<IMediaActionEmits>();
const minCount = computed(() => {
	return props.truncated ? 1 : 100;
});

const getQualityDisplay = (quality: VideoQuality): {
	color: string;
	label: string;
} => {
	switch (quality) {
		case VideoQuality.None:
			return {
				color: 'black',
				label: translateVideoQuality(VideoQuality.None),
			};
		case VideoQuality.SubSD144P:
			return {
				color: 'brown-6',
				label: translateVideoQuality(VideoQuality.SubSD144P),
			};
		case VideoQuality.SubSDCIF:
			return {
				color: 'deep-orange-6',
				label: translateVideoQuality(VideoQuality.SubSDCIF),
			};
		case VideoQuality.NHD:
			return {
				color: 'orange-7',
				label: translateVideoQuality(VideoQuality.NHD),
			};
		case VideoQuality.SD:
			return {
				color: 'amber-7',
				label: translateVideoQuality(VideoQuality.SD),
			};
		case VideoQuality.DVD:
			return {
				color: 'yellow-7',
				label: translateVideoQuality(VideoQuality.DVD),
			};
		case VideoQuality.HD:
			return {
				color: 'light-green-13',
				label: translateVideoQuality(VideoQuality.HD),
			};
		case VideoQuality.FullHD:
			return {
				color: 'light-blue-6',
				label: translateVideoQuality(VideoQuality.FullHD),
			};
		case VideoQuality.QHD:
			return {
				color: 'cyan-6',
				label: translateVideoQuality(VideoQuality.QHD),
			};
		case VideoQuality.UHD_4K:
			return {
				color: 'red darken-4',
				label: translateVideoQuality(VideoQuality.UHD_4K),
			};
		case VideoQuality.UHD_8K:
			return {
				color: 'purple-8',
				label: translateVideoQuality(VideoQuality.UHD_8K),
			};
		case VideoQuality.Unknown:
			return {
				color: 'blue-grey-4',
				label: translateVideoQuality(VideoQuality.Unknown),
			};
		default:
			Log.error('Missing quality display mapping for', quality);
			return {
				color: 'blue-grey-4',
				label: translateVideoQuality(VideoQuality.None),
			};
	}
};
</script>
