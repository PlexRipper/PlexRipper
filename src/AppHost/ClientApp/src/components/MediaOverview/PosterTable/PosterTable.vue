<template>
	<!-- Poster display -->
	<div
		id="poster-table"
		ref="scrollContainerRef"
		:style="{ paddingLeft: `${gridPaddingLeft}px` }"
		data-cy="poster-table">
		<!-- Total height spacer — required by TanStack Virtual to define the scrollable area -->
		<div
			:data-pages-version="mediaOverviewStore.mediaPagesVersion"
			:style="{ height: `${safeTotalSize}px`, position: 'relative' }">
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
					v-for="rowItem in getRowItems(virtualRow.index)"
					:key="rowItem.item.id"
					:media-item="rowItem.item"
					:active="true"
					:data-scroll-index="rowItem.index"
					@download="sendMediaOverviewDownloadCommand($event)"
					@open-media-details="onOpenMediaDetails" />
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useVirtualizer } from '@tanstack/vue-virtual';

import { get, set, useElementBounding } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexMediaSlimDTO, PlexMediaType } from '@dto';
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
const pageRequestPending = ref(false);
const hasRunInitialPageReady = ref(false);
const router = useRouter();

defineProps<{
	mediaType: PlexMediaType;
	libraryId: number;
}>();

// Number of rows = ceil(total items / columns)
const rowCount = computed(() => Math.ceil(mediaOverviewStore.totalCount / get(gridItems)));

// Row virtualizer — re-configures reactively when rowCount or posterCardHeight changes
const rowVirtualizer = useVirtualizer(
	computed(() => ({
		count: get(rowCount),
		getScrollElement: () => get(scrollContainerRef),
		estimateSize: () => get(posterCardHeight),
		// Render extra rows above and below viewport for smoother jumps and less blanking while scrolling
		overscan: 10,
		// Stable row keys: use the first item id in each row
		getItemKey: (rowIndex: number): number => {
			const firstItem = mediaOverviewStore.getMediaItemsForRange(rowIndex * get(gridItems), rowIndex * get(gridItems) + 1).at(0);
			return firstItem?.id ?? rowIndex;
		},
		onChange: (_instance: unknown, sync: boolean) => {
			if (sync)
				return;

			requestPagesAroundViewport();
		},
	})),
);

// Firefox and Chrome silently clamp CSS element heights at ~33.5M px, causing a blank render.
// This guard ensures the spacer div never exceeds that limit regardless of item count or column count.
const BROWSER_MAX_CSS_HEIGHT = 33_000_000;
const safeTotalSize = computed(() => Math.min(rowVirtualizer.value.getTotalSize(), BROWSER_MAX_CSS_HEIGHT));


// Returns the loaded items belonging to a given row index, preserving each item's global index.
function getRowItems(rowIndex: number): { item: PlexMediaSlimDTO; index: number }[] {
	const cols = get(gridItems);
	const startIndex = rowIndex * cols;
	return mediaOverviewStore.getMediaItemsForRange(startIndex, startIndex + cols)
		.map((item, itemIndex) => ({ item, index: startIndex + itemIndex }));
}

// useElementBounding must be called at setup level so its ResizeObserver is wired correctly.
// Calling it inside watchEffect/watch creates a new instance each time with width=0, which
// causes rowCount = ceil(N/1) = N rows and getTotalSize() to exceed browser CSS height limits.
const { width: containerWidth } = useElementBounding(scrollContainerRef);

// Recalculate columns and padding whenever container width changes
watch(containerWidth, (width) => {
	const cols = Math.max(1, Math.floor(width / get(posterCardWidth)));
	set(gridItems, cols);
	set(gridPaddingLeft, (width - cols * get(posterCardWidth)) / 2);

	if (!get(hasRunInitialPageReady)) {
		set(hasRunInitialPageReady, true);
		nextTick(() => onPageReady());
	}
});

function onPageReady() {
	const lastMediaItemViewed = get(mediaOverviewStore.lastMediaItemViewed);
	if (lastMediaItemViewed && lastMediaItemViewed.sortIndex > 0) {
		// If we have a last viewed media item, scroll to it
		scrollToIndex(lastMediaItemViewed.sortIndex - 1);
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

function requestPagesAroundViewport() {
	if (get(pageRequestPending) || mediaOverviewStore.getMediaItems.length >= mediaOverviewStore.totalCount)
		return;

	const virtualItems = get(rowVirtualizer).getVirtualItems();
	const firstVirtualRow = virtualItems.at(0);
	const lastVirtualRow = virtualItems.at(-1);
	if (!firstVirtualRow || !lastVirtualRow)
		return;

	const cols = get(gridItems);
	const firstVisibleIndex = firstVirtualRow.index * cols;
	const lastVisibleIndex = ((lastVirtualRow.index + 1) * cols) - 1;
	const prefetchBuffer = mediaOverviewStore.pageSize;
	const prefetchStart = Math.max(0, firstVisibleIndex - prefetchBuffer);
	const prefetchEnd = Math.min(mediaOverviewStore.totalCount, lastVisibleIndex + prefetchBuffer);

	set(pageRequestPending, true);
	useSubscription(
		mediaOverviewStore.requestRange(prefetchStart, prefetchEnd).subscribe({
			next: () => set(pageRequestPending, false),
			error: () => set(pageRequestPending, false),
			complete: () => set(pageRequestPending, false),
		}),
	);
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
		if (!element) {
			Log.debug('Could not find element to highlight for scroll index:', index);
			return;
		}

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

		if (scrollIndex < 0 || scrollIndex >= mediaOverviewStore.totalCount) {
			Log.warn(`Scroll index ${scrollIndex} is out of bounds for total count ${mediaOverviewStore.totalCount}`);
			return;
		}

		// Scroll immediately for responsiveness, then prefetch nearby pages in background
		scrollToIndex(scrollIndex);

		set(pageRequestPending, true);
		useSubscription(
			mediaOverviewStore.requestAroundIndex(scrollIndex).subscribe({
				next: () => set(pageRequestPending, false),
				error: () => set(pageRequestPending, false),
				complete: () => set(pageRequestPending, false),
			}),
		);
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
