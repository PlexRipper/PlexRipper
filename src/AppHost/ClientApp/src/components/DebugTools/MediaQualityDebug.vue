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
import { type PlexMediaQualityDTO, PlexMediaType, VideoQuality } from '@dto';
import QSection from '@components/Common/QSection.vue';

const qualities: PlexMediaQualityDTO[] = [
	{ quality: VideoQuality.None, dataId: 0, mediaDataType: PlexMediaType.None, mediaId: 0 },
	{ quality: VideoQuality.Unknown, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.SubSD144P, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.SubSDCIF, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.NHD, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.SD, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.DVD, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.HD, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.FullHD, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.QHD, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.UHD_4K, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
	{ quality: VideoQuality.UHD_8K, dataId: 0, mediaDataType: PlexMediaType.Unknown, mediaId: 0 },
];
</script>
