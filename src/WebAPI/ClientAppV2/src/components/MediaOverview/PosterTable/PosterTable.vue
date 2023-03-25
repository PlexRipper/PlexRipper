<template>
	<q-row>
		<q-col class="poster-overview">
			<!-- Poster display-->
			<QScrollArea ref="scrollbarposters" class="fit">
				<q-row justify="center">
					<template v-for="item in items" :key="item.id">
						<media-poster
							:media-item="item"
							:media-type="mediaType"
							@download="downloadMedia"
							@open-details="$emit('open-details', $event)" />
					</template>
				</q-row>
			</QScrollArea>
		</q-col>
		<!-- Alphabet Navigation-->
		<alphabet-navigation :items="items" @scroll-to="scrollToIndex" />
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, defineEmits, ref } from 'vue';
import { DownloadMediaDTO, PlexMediaType } from '@dto/mainApi';
import ITreeViewItem from '@class/ITreeViewItem';
import { QScrollArea } from '#components';

const props = defineProps<{
	items: ITreeViewItem[];
	mediaType: PlexMediaType;
	activeAccountId: number;
}>();

const emit = defineEmits<{
	(e: 'open-details', mediaId: number): void;
	(e: 'download', downloadMediaCommands: DownloadMediaDTO[]): void;
}>();

const scrollbarposters = ref<InstanceType<typeof QScrollArea> | null>(null);

const downloadMedia = (downloadMediaCommands: DownloadMediaDTO[]): void => {
	downloadMediaCommands.forEach((x) => {
		x.plexAccountId = props.activeAccountId;
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
.poster-overview,
.alphabet-navigation {
	height: calc(100vh - 85px - 48px);
	width: 100%;
	// 8% reserved for the media-overview-bar
	// height: 92%;
}
</style>
