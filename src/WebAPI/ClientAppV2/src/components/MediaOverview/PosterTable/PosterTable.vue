<template>
	<!-- Poster display-->
	<q-row justify="center">
		<template v-for="item in posters" :key="item.id">
			<media-poster
				:media-item="item"
				:media-type="mediaType"
				@download="downloadMedia($event)"
				@open-details="$emit('open-details', $event)" />
		</template>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, defineEmits, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { QScrollArea } from '#components';
import { MediaService } from '@service';

const props = defineProps<{
	mediaType: PlexMediaType;
	libraryId: number;
	items: PlexMediaSlimDTO[];
}>();

const emit = defineEmits<{
	(e: 'open-details', mediaId: number): void;
	(e: 'download', downloadMediaCommands: DownloadMediaDTO[]): void;
}>();

const posters = ref<PlexMediaDTO[]>([]);
const loading = ref(true);

const scrollbarposters = ref<InstanceType<typeof QScrollArea> | null>(null);

const downloadMedia = (downloadMediaCommands: DownloadMediaDTO[]): void => {
	downloadMediaCommands.forEach((x) => {
		// x.plexAccountId = props.activeAccountId ?? 0;
	});
	emit('download', downloadMediaCommands);
};

const scrollToIndex = (letter: string) => {
	if (!scrollbarposters.value) {
		Log.error('Could not find container with reference: ' + scrollbarposters.value);
		return;
	}

	let scrollHeight = 0;
	if (letter !== '#') {
		const children = scrollbarposters.value.$children[0].$children;
		const index = children.findIndex((x) => (x.$el as HTMLElement)?.getAttribute('data-title')?.startsWith(letter)) ?? -1;
		if (index > -1 && children[index]) {
			scrollHeight = (children[index].$el as HTMLElement).offsetTop;
		} else {
			Log.error('Could not find an index with letter ' + letter);
		}
	}

	scrollbarposters.value.scrollTo({ y: scrollHeight }, 500);
};
</script>
<style lang="scss">
//.poster-overview,
//.alphabet-navigation {
//	height: calc(100vh - 85px - 48px);
//	width: 100%;
//	// 8% reserved for the media-overview-bar
//	// height: 92%;
//}
</style>
