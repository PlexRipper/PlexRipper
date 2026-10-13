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
	audioCodec?: string | null;
	audioBitrateKbps?: number | null;
	audioBitDepth?: number | null;
	audioSampleRateHz?: number | null;
	audioChannels?: number | null;
	audioChannelLayout?: string | null;
	audioProfile?: string | null;
}>(), {
	clickable: false,
	audioCodec: null,
	audioBitrateKbps: null,
	audioBitDepth: null,
	audioSampleRateHz: null,
	audioChannels: null,
	audioChannelLayout: null,
	audioProfile: null,
});
const { t } = useI18n();

const audioDetails = computed(() => [
	props.audioCodec?.trim(),
	props.audioBitrateKbps != null && props.audioBitrateKbps > 0 ? `${props.audioBitrateKbps} kbps` : null,
	props.audioBitDepth != null && props.audioBitDepth > 0 ? `${props.audioBitDepth}-bit` : null,
	props.audioSampleRateHz != null && props.audioSampleRateHz > 0 ? `${formatSampleRate(props.audioSampleRateHz)} kHz` : null,
	props.audioChannels != null && props.audioChannels > 0 ? `${props.audioChannels} channels` : null,
	props.audioChannelLayout?.trim(),
	props.audioProfile?.trim(),
].filter((detail): detail is string => Boolean(detail)).join(' · '));

const label = computed(() => {
	const qualityLabel = props.quality === AudioQuality.Unknown
		? t('general.error.unknown')
		: props.quality.replace(/(\d+)_(\d+)kHz/, '$1.$2 kHz')
			.replace(/(\d+)kHz/, '$1 kHz')
			.replace(/_Below/, ' <').replace(/_/g, ' ')
			.replace(/Bit/g, '-bit').replace(/KbpsPlus/, ' kbps+').replace(/Kbps/, ' kbps');
	return audioDetails.value ? `${qualityLabel} · ${audioDetails.value}` : qualityLabel;
});

function formatSampleRate(sampleRateHz: number): string {
	return (sampleRateHz / 1000).toString();
}
</script>
