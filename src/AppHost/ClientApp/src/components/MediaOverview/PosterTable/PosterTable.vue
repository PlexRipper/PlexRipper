<template>
	<!-- Poster display -->
	<div
		id="poster-table"
		ref="scrollContainerRef"
		:style="{ paddingLeft: `${gridPaddingLeft}px` }"
		data-cy="poster-table">
		<!-- Total height spacer — required by TanStack Virtual to define the scrollable area -->
		<div :style="{ height: `${rowVirtualizer.getTotalSize()}px`, position: 'relative' }">
			<!-- Only virtual rows are rendered, positioned absolutely via translateY -->
			<div
				v-for="virtualRow in rowVirtualizer.getVirtualItems()"
				:key="virtualRow.index"
				:style="{
					position: 'absolute',
					top: 0,
					left: 0,
					width: '100%',
					transform: `translateY(${virtualRow.start}px)`,
					display: 'flex',
				}">
				<MediaPoster
					v-for="item in getRowItems(virtualRow.index)"
					:key="item.id"
					:media-item="item"
					:active="true"
					:data-scroll-index="getItemFlatIndex(virtualRow.index, item)"
					@download="sendMediaOverviewDownloadCommand($event)"
					@open-media-details="onOpenMediaDetails" />
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useVirtualizer } from '@tanstack/vue-virtual';

import { get, set } from '@vueuse/core';
import type { PlexMediaType, PlexMediaSlimDTO } from '@dto';
import { listenMediaOverviewScrollToCommand, sendMediaOverviewDownloadCommand } from '@composables/event-bus';
import { triggerBoxHighlight } from '@composables/animations';
import { waitForElement } from '@composables';
import { useRouter, useMediaOverviewStore } from '#imports';

const mediaOverviewStore = useMediaOverviewStore();

const scrollContainerRef = ref<HTMLElement | null>(null);
const posterCardWidth = ref(200 + 32);
const posterCardHeight = ref(340 + 32);
const gridItems = ref(10);
const gridPaddingLeft = ref(0);
const router = useRouter();

const props = defineProps<{
	mediaType: PlexMediaType;
	libraryId: number;
	items: Readonly<PlexMediaSlimDTO[]>;
}>();

// Number of rows = ceil(total items / columns)
const rowCount = computed(() => Math.ceil(props.items.length / get(gridItems)));

// Row virtualizer — re-configures reactively when rowCount or posterCardHeight changes
const rowVirtualizer = useVirtualizer(
	computed(() => ({
		count: get(rowCount),
		getScrollElement: () => get(scrollContainerRef),
		estimateSize: () => get(posterCardHeight),
		// Render 5 extra rows above and below viewport for smooth scrolling
		overscan: 5,
		// Stable row keys: use the first item id in each row
		getItemKey: (rowIndex: number): number => {
			const firstItem = props.items[rowIndex * get(gridItems)];
			return firstItem?.id ?? rowIndex;
		},
	})),
);

// Returns the items belonging to a given row index
function getRowItems(rowIndex: number): PlexMediaSlimDTO[] {
	const cols = get(gridItems);
	return props.items.slice(rowIndex * cols, (rowIndex + 1) * cols) as PlexMediaSlimDTO[];
}

// Converts (rowIndex, item) back to the flat item index for scroll restoration and highlight
function getItemFlatIndex(rowIndex: number, item: PlexMediaSlimDTO): number {
	const cols = get(gridItems);
	const rowItems = getRowItems(rowIndex);
	return rowIndex * cols + rowItems.indexOf(item);
}

// Recalculate columns and padding whenever container width changes
watchEffect(() => {
	const el = get(scrollContainerRef);
	if (!el) return;
	const { width } = useElementBounding(scrollContainerRef);
	const cols = Math.max(1, Math.floor(get(width) / get(posterCardWidth)));
	set(gridItems, cols);
	set(gridPaddingLeft, (get(width) - cols * get(posterCardWidth)) / 2);
	nextTick(() => onPageReady());
});

function onPageReady() {
	const lastMediaItemViewed = get(mediaOverviewStore.lastMediaItemViewed);
	if (lastMediaItemViewed) {
		// The index is relative depending on the view mode so we translate the mediaId to the index of the current view
		const index = mediaOverviewStore.getMediaIndex(lastMediaItemViewed?.id);
		// If we have a last viewed media item, scroll to it
		scrollToIndex(index);
	}
}

function onOpenMediaDetails(mediaItem: PlexMediaSlimDTO) {
	router.push({
		name: 'tvshows-libraryId-details-tvShowId',
		params: {
			libraryId: mediaItem.plexLibraryId.toString(),
			tvShowId: mediaItem.id.toString(),
		},
	});
}

function scrollToIndex(index: number) {
	const container = get(scrollContainerRef);
	if (!container) {
		Log.error('Could not find scroll container reference: ', container);
		return;
	}

	Log.debug('Scrolling to index:', index);

	// Convert flat item index to row index, then scroll to that row
	const rowIndex = Math.floor(index / get(gridItems));
	get(rowVirtualizer).scrollToIndex(rowIndex, { align: 'start' });

	// Wait for the element to be rendered before highlighting
	waitForElement(container, `[data-scroll-index="${index}"]`).then((element) => {
		// Highlight the element after a short delay due to render hang
		setTimeout(() => {
			triggerBoxHighlight(element);
		}, 400);
	});
}

onMounted(() => {
	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((scrollIndex) => {
		if (!get(scrollContainerRef)) {
			Log.error('Could not find container with reference: ', get(scrollContainerRef));
			return;
		}

		if (scrollIndex < 0 || scrollIndex >= props.items.length) {
			Log.warn(`Scroll index ${scrollIndex} is out of bounds for items length ${props.items.length}`);
			return;
		}

		scrollToIndex(scrollIndex);
	});
});
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

#poster-table {
  overflow-y: auto;
  overflow-x: hidden;
  // Required for absolute positioning of virtual rows inside the spacer div
  position: relative;

  max-height: calc($page-height-minus-app-bar-minus-media-overview-bar);
}

.poster-table-item {
  // GPU acceleration for each item — smoother scrolling
  will-change: transform;
  // Prevent layout thrashing during scroll
  contain: layout style paint;
}
</style>
