<template>
	<!-- Poster display -->
	<div
		id="poster-table"
		ref="scrollContainerRef"
		:style="{ paddingLeft: `${gridPaddingLeft}px` }"
		data-cy="poster-table">
		<!-- Total height spacer — required by TanStack Virtual to define the scrollable area -->
		<div :style="{ height: `${safeTotalSize}px`, position: 'relative' }">
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

import { get, set, useElementBounding } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
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
const pageRequestPending = ref(false);
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
		onChange: (_instance: unknown, sync: boolean) => {
			if (sync)
				return;

			requestNextPageNearBottom();
		},
	})),
);

// Firefox and Chrome silently clamp CSS element heights at ~33.5M px, causing a blank render.
// This guard ensures the spacer div never exceeds that limit regardless of item count or column count.
const BROWSER_MAX_CSS_HEIGHT = 33_000_000;
const safeTotalSize = computed(() => Math.min(rowVirtualizer.value.getTotalSize(), BROWSER_MAX_CSS_HEIGHT));

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

// useElementBounding must be called at setup level so its ResizeObserver is wired correctly.
// Calling it inside watchEffect/watch creates a new instance each time with width=0, which
// causes rowCount = ceil(N/1) = N rows and getTotalSize() to exceed browser CSS height limits.
const { width: containerWidth } = useElementBounding(scrollContainerRef);

// Recalculate columns and padding whenever container width changes
watch(containerWidth, (width) => {
	const cols = Math.max(1, Math.floor(width / get(posterCardWidth)));
	set(gridItems, cols);
	set(gridPaddingLeft, (width - cols * get(posterCardWidth)) / 2);
	nextTick(() => onPageReady());
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

function requestNextPageNearBottom() {
	if (get(pageRequestPending) || props.items.length >= mediaOverviewStore.totalCount)
		return;

	const lastVirtualRow = get(rowVirtualizer).getVirtualItems().at(-1);
	if (!lastVirtualRow)
		return;

	const lastVisibleIndex = (lastVirtualRow.index + 1) * get(gridItems) - 1;
	if (lastVisibleIndex < props.items.length - get(gridItems) * 2)
		return;

	set(pageRequestPending, true);
	const nextPage = Math.floor(props.items.length / mediaOverviewStore.pageSize) + 1;
	useSubscription(
		mediaOverviewStore.requestMediaPage(nextPage).subscribe({
			next: () => set(pageRequestPending, false),
			error: () => set(pageRequestPending, false),
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

		useSubscription(
			mediaOverviewStore.requestAroundIndex(scrollIndex).subscribe(() => {
				scrollToIndex(scrollIndex);
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
