<template>
	<QSection :header="$t('pages.debug.buttons.media-quality-header')">
		<QRow
			justify="between"
			class="q-my-md">
			<MediaQuality
				:qualities="qualities"
				:truncated="false" />
		</QRow>
		<QRow
			justify="between"
			class="q-my-md">
			<MediaQuality
				:qualities="qualities"
				:truncated="true" />
		</QRow>
	</QSection>
</template>

<script setup lang="ts">
import { AudioQuality, type PlexMediaQualityDTO, PlexMediaType, VideoQuality } from '@dto';
import QSection from '@components/Common/QSection.vue';

const qualities: PlexMediaQualityDTO[] = [
	{ quality: VideoQuality.None, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.None, mediaId: 0 },
	{ quality: VideoQuality.Unknown, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.SubSD144P, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.SubSDCIF, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.NHD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.SD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.DVD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.HD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.FullHD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.QHD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.UHD_4K, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.UHD_8K, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
];
</script>
