<template>
	<QGlowChip
		v-if="quality !== AudioQuality.None"
		:color="quality === AudioQuality.Unknown ? 'blue-grey-4' : 'cyan-6'"
		:clickable="clickable"
		size="md"
		has-background
		:value="label" />
</template>

<script setup lang="ts">
import { AudioQuality } from '@dto';

const props = withDefaults(defineProps<{
	quality: AudioQuality;
	clickable?: boolean;
}>(), {
	clickable: false,
});
const { t } = useI18n();

const label = computed(() => props.quality === AudioQuality.Unknown
	? t('general.error.unknown')
	: props.quality.replace(/(\d+)_(\d+)kHz/, '$1.$2 kHz')
		.replace(/(\d+)kHz/, '$1 kHz')
		.replace(/_Below/, ' <').replace(/_/g, ' ')
		.replace(/Bit/g, '-bit').replace(/KbpsPlus/, ' kbps+').replace(/Kbps/, ' kbps'));
</script>
