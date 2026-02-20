<template>
	<div
		class="media-table"
		data-cy="media-table">
		<MediaTableHeader
			:columns="mediaTableColumns"
			selectable
			:selected="mediaOverviewStore.isRootSelected"
			class="media-table--header"
			@selected="mediaOverviewStore.setRootSelected($event)" />
		<div
			id="media-table-scroll"
			ref="qTableRef"
			:class="['media-table--content', isScrollable ? 'scroll' : '']"
			data-cy="media-table-scroll">
			<!-- Total height spacer — required by TanStack Virtual to define the scrollable area -->
			<div :style="{ height: `${safeTotalSize}px`, position: 'relative' }">
				<!-- Only virtual rows are rendered, positioned absolutely via translateY -->
				<div
					v-for="virtualRow in rowVirtualizer.getVirtualItems()"
					:key="virtualRow.key"
					:style="{
						position: 'absolute',
						top: 0,
						left: 0,
						width: '100%',
						transform: `translateY(${virtualRow.start}px)`,
					}"
					class="media-table--intersection highlight-border-box"
					:data-scroll-index="virtualRow.index">
					<MediaTableRow
						:index="virtualRow.index"
						:data-cy="`media-table-row-${virtualRow.index}`"
						:columns="mediaTableColumns"
						:row="rows[virtualRow.index]"
						selectable
						:selected="isSelected(rows[virtualRow.index].id)"
						:disable-highlight="disableHighlight"
						:disable-hover-click="disableHoverClick"
						@selected="updateSelectedRow(rows[virtualRow.index].id, $event)" />
				</div>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useVirtualizer } from '@tanstack/vue-virtual';
import { get, set } from '@vueuse/core';
import type { PlexMediaSlimDTO } from '@dto';
import type { ISelection } from '@interfaces';
import {
	triggerBoxHighlight,
	listenMediaOverviewScrollToCommand,
	useMediaOverviewStore,
} from '#imports';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';

const mediaOverviewStore = useMediaOverviewStore();
const mediaTableColumns = getMediaTableColumns();
const qTableRef = ref<HTMLElement | null>(null);
const scrollTargetElement = ref<HTMLElement | null>(null);
const autoScrollEnabled = ref(false);

const props = withDefaults(
	defineProps<{
		rows: Readonly<PlexMediaSlimDTO[]>;
		disableHoverClick?: boolean;
		disableHighlight?: boolean;
		isScrollable?: boolean;
	}>(),
	{
		disableHoverClick: false,
		disableHighlight: false,
		isScrollable: true,
	},
);

defineEmits<{
	(e: 'row-click', payload: PlexMediaSlimDTO): void;
}>();

// Row height matches $media-table-row-height in _variables.scss
const ROW_HEIGHT = 42;

// Firefox and Chrome silently clamp CSS element heights at ~33.5M px, causing a blank render.
// This guard ensures the spacer div never exceeds that limit regardless of item count.
const BROWSER_MAX_CSS_HEIGHT = 33_000_000;

const rowVirtualizer = useVirtualizer(
	computed(() => ({
		count: props.rows.length,
		getScrollElement: () => get(qTableRef),
		estimateSize: () => ROW_HEIGHT,
		overscan: 10,
		getItemKey: (index: number) => props.rows[index]?.id ?? index,
	})),
);

const safeTotalSize = computed(() => Math.min(rowVirtualizer.value.getTotalSize(), BROWSER_MAX_CSS_HEIGHT));

function isSelected(mediaId: number) {
	return (mediaOverviewStore.selection?.keys ?? []).includes(mediaId);
}

function updateSelectedRow(mediaId: number, state: boolean) {
	mediaOverviewStore.setSelection({
		...mediaOverviewStore.selection,
		keys: state
			? [...(mediaOverviewStore.selection?.keys ?? []), mediaId]
			: (mediaOverviewStore.selection?.keys ?? []).filter((x) => x !== mediaId),
		allSelected: false,
	} as ISelection);
}

function scrollToIndex(index: number) {
	const container = get(qTableRef);
	if (!container) {
		Log.error(`Could not find scroll container reference`);
		return;
	}

	set(autoScrollEnabled, true);
	get(rowVirtualizer).scrollToIndex(index, { align: 'start' });

	// Wait for the virtual row to be rendered, then highlight it
	nextTick(() => {
		const element: HTMLElement | null = container.querySelector(`[data-scroll-index="${index}"]`) ?? null;
		if (!element) {
			Log.error(`Could not find scroll target element`, `[data-scroll-index="${index}"]`);
			return;
		}
		set(scrollTargetElement, element);

		const elementRect = element.getBoundingClientRect();
		if (elementRect.bottom >= 0 && elementRect.top <= window.innerHeight) {
			triggerBoxHighlight(element);
		} else {
			element.scrollIntoView({ block: 'start', behavior: 'smooth' });
		}
	});
}

onMounted(() => {
	const lastMediaItemViewed = get(mediaOverviewStore.lastMediaItemViewed);
	if (lastMediaItemViewed && lastMediaItemViewed.sortIndex > 0) {
		// If we have a last viewed media item, scroll to it
		scrollToIndex(lastMediaItemViewed.sortIndex - 1);
	}

	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(qTableRef)) {
			Log.error('qTableRef is null');
			return;
		}

		mediaOverviewStore.clearSort();
		const index = mediaOverviewStore.scrollDict[letter] ? mediaOverviewStore.scrollDict[letter] : 0;
		scrollToIndex(index);
	});
});
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.media-table {
  &--header,
  &--intersection,
  &--intersection > div {
    height: $media-table-row-height;
  }

  &--content {
    max-height: calc($page-height-minus-app-bar - $media-overview-bar-height - $media-table-row-height);
  }
}

.row-title {
  font-weight: bold;
  min-width: 300px;
  max-width: 300px;

  &--hover {
    cursor: pointer;

    :hover {
      color: $primary;
    }
  }
}
</style>
