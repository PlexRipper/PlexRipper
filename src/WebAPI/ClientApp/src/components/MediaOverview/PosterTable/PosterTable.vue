<template>
	<!-- Poster display -->
	<RecycleScroller
		id="poster-table"
		v-slot="{ item, index }"
		ref="recycleScrollerRef"
		:items="items"
		:item-size="posterCardHeight"
		:item-secondary-size="posterCardWidth"
		:grid-items="gridItems"
		:buffer="posterCardHeight * 5"
		key-field="id"
		data-cy="poster-table"
		@resize="onResize">
		<MediaPoster
			:index="index"
			:media-item="item"
			:data-scroll-index="index"
			@download="sendMediaOverviewDownloadCommand($event)" />
	</RecycleScroller>
</template>

<script setup lang="ts">
import Log from 'consola';
import { RecycleScroller } from 'vue-virtual-scroller';
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css';

import { get, set } from '@vueuse/core';
import type { PlexMediaType, PlexMediaSlimDTO } from '@dto';
import { listenMediaOverviewScrollToCommand, sendMediaOverviewDownloadCommand } from '@composables/event-bus';
import { triggerBoxHighlight } from '@composables/animations';
import { waitForElement } from '@composables';

const mediaOverviewStore = useMediaOverviewStore();

const autoScrollEnabled = ref(false);
const recycleScrollerRef = ref<RecycleScroller | null>(null);
const posterTableRef = computed(() => document.getElementById('poster-table') ?? null);
const posterCardWidth = ref(200 + 32);
const posterCardHeight = ref(340 + 32);
const gridItems = ref(0);
const scrolledIndex = ref(0);

defineProps<{
	mediaType: PlexMediaType;
	libraryId: number;
	items: Readonly<PlexMediaSlimDTO[]>;
}>();

function onResize() {
	const { width } = useElementBounding(posterTableRef);
	set(gridItems, Math.floor(get(width) / get(posterCardWidth)));
}

onMounted(() => {
	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(posterTableRef)) {
			Log.error('Could not find container with reference: ', get(posterTableRef));
			return;
		}
		// We have to revert to normal title sort otherwise the index will be wrong
		mediaOverviewStore.clearSort();

		const index = mediaOverviewStore.scrollDict[letter] ?? 0;
		set(scrolledIndex, index);
		set(autoScrollEnabled, true);

		const scrollRef = get(recycleScrollerRef);
		if (!scrollRef) {
			Log.error('Could not find recycle scroller reference: ', scrollRef);
			return;
		}

		// Scroll to item first, otherwise the target element won't exist in dom to highlight
		scrollRef.scrollToItem(index);

		// Wait for the element to be rendered before highlighting
		waitForElement(get(posterTableRef), `[data-scroll-index="${index}"]`).then((element) => {
			// Highlight the element after a short delay due to render hang
			setTimeout(() => {
				triggerBoxHighlight(element);
			}, 400);
		});
	});
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

#poster-table {
	overflow-y: auto;
	overflow-x: hidden;

	max-height: calc(100vh - $app-bar-height - $media-overview-bar-height);

	&--scroll-container {
		height: 100%;
		width: 100%;
	}
}
</style>
