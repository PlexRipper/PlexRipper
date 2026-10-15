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
	{ videoQuality: VideoQuality.None, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.None, mediaId: 0 },
	{ videoQuality: VideoQuality.Unknown, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.SubSD144P, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.SubSDCIF, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.NHD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.SD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.DVD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.HD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.FullHD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.QHD, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.UHD_4K, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ videoQuality: VideoQuality.UHD_8K, audioQuality: AudioQuality.None, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
];
</script>
