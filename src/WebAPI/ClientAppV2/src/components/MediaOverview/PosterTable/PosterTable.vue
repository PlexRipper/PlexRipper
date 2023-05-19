<template>
	<!-- Poster display-->
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
		@resize="onResize">
		<media-poster
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

import { get, set, useScroll } from '@vueuse/core';
import { PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import {
	listenMediaOverviewScrollToCommand,
	sendMediaOverviewDownloadCommand,
	setMediaOverviewSort,
} from '@composables/event-bus';
import { triggerBoxHighlight } from '@composables/animations';

const autoScrollEnabled = ref(false);
const recycleScrollerRef = ref<RecycleScroller | null>(null);
const posterTableRef = computed(() => document.getElementById('poster-table') ?? null);
const posterCardWidth = ref(200 + 32);
const posterCardHeight = ref(340 + 32);
const gridItems = ref(0);
const scrolledIndex = ref(0);

const props = withDefaults(
	defineProps<{
		mediaType: PlexMediaType;
		libraryId: number;
		items: PlexMediaSlimDTO[];
		scrollDict?: Record<string, number>;
	}>(),
	{
		scrollDict: { '#': 0 } as any,
	},
);

defineEmits<{
	(e: 'load', payload: any): void;
}>();

const onResize = useDebounceFn(
	() => {
		const { width } = useElementBounding(posterTableRef);
		set(gridItems, Math.floor(get(width) / get(posterCardWidth)));
	},
	500,
	{ maxWait: 5000 },
);

function getScrollTarget(index: number): HTMLElement | null {
	// noinspection TypeScriptValidateTypes
	const element: HTMLElement | null = get(posterTableRef)?.querySelector(`[data-scroll-index="${index}"]`) ?? null;
	if (!element) {
		Log.error(`Could not find scroll target element for letter with index ${index}`, `[data-scroll-index="${index}"]`);
		return null;
	}
	return element;
}

onMounted(() => {
	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(posterTableRef)) {
			Log.error('Could not find container with reference: ', get(posterTableRef));
			return;
		}
		if (!props.scrollDict) {
			Log.error('Could not find scrollDict');
			return;
		}
		// We have to revert to normal title sort otherwise the index will be wrong
		setMediaOverviewSort({ sort: 'asc', field: 'sortTitle' });
		const index = props.scrollDict[letter] ?? 0;
		set(scrolledIndex, index);
		set(autoScrollEnabled, true);

		// Scroll to item first, otherwise the target element won't exist in dom to highlight
		const beforeScroll = get(recycleScrollerRef)?.getScroll();
		get(recycleScrollerRef)?.scrollToItem(index);
		const afterScroll = get(recycleScrollerRef)?.getScroll();

		// No scroll happened, trigger highlight manually
		if (beforeScroll.end === afterScroll.end) {
			triggerBoxHighlight(getScrollTarget(index));
		}
	});

	// Setup stopped scrolling event listener
	useScroll(get(posterTableRef), {
		onStop() {
			// Don't highlight if the user scrolls manually
			if (!get(autoScrollEnabled)) {
				return;
			}
			set(autoScrollEnabled, false);

			// noinspection TypeScriptValidateTypes
			const element: HTMLElement | null = getScrollTarget(get(scrolledIndex));

			triggerBoxHighlight(element);
		},
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
