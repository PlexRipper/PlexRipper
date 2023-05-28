<template>
	<div class="media-table" data-cy="media-table">
		<MediaTableHeader
			:columns="mediaTableColumns"
			selectable
			:selected="rootSelected"
			class="media-table--header"
			@selected="rootSetSelected($event)" />
		<div
			id="media-table-scroll"
			ref="qTableRef"
			:class="['media-table--content', isScrollable ? 'scroll' : '']"
			data-cy="media-table-scroll">
			<template v-if="disableIntersection">
				<MediaTableRow
					v-for="(row, index) in rows"
					:key="index"
					:index="index"
					:data-cy="`media-table-row-${index}`"
					:columns="mediaTableColumns"
					:row="row"
					selectable
					:selected="isSelected(row.id)"
					:disable-highlight="disableHighlight"
					:disable-hover-click="disableHoverClick"
					@selected="updateSelectedRow(row.id, $event)" />
			</template>
			<template v-else>
				<q-intersection
					v-for="(row, index) in rows"
					:key="index"
					:once="disableIntersection"
					class="media-table--intersection highlight-border-box"
					:data-scroll-index="index">
					<MediaTableRow
						:index="index"
						:data-cy="`media-table-row-${index}`"
						:columns="mediaTableColumns"
						:row="row"
						selectable
						:selected="isSelected(row.id)"
						:disable-highlight="disableHighlight"
						:disable-hover-click="disableHoverClick"
						@selected="updateSelectedRow(row.id, $event)" />
				</q-intersection>
			</template>
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set, useScroll } from '@vueuse/core';
import { setMediaOverviewSort, triggerBoxHighlight, listenMediaOverviewScrollToCommand } from '#imports';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';

const mediaTableColumns = getMediaTableColumns();
const qTableRef = ref<HTMLElement | null>(null);
const scrollTargetElement = ref<HTMLElement | null>(null);
const autoScrollEnabled = ref(false);

const props = withDefaults(
	defineProps<{
		rows: PlexMediaSlimDTO[];
		selection: ISelection | null;
		scrollDict?: Record<string, number>;
		disableHoverClick?: boolean;
		disableHighlight?: boolean;
		disableIntersection?: boolean;
		isScrollable?: boolean;
	}>(),
	{
		scrollDict: { '#': 0 } as any,
		disableHoverClick: false,
		disableHighlight: false,
		disableIntersection: false,
		isScrollable: true,
	},
);

const emit = defineEmits<{
	(e: 'selection', payload: ISelection): void;
	(e: 'row-click', payload: PlexMediaSlimDTO): void;
}>();

function isSelected(mediaId: number) {
	return (props.selection?.keys ?? []).includes(mediaId);
}

const rootSelected = computed((): boolean | null => {
	if (props.selection?.keys.length === props.rows.length) {
		return true;
	}

	if (props.selection?.keys.length === 0) {
		return false;
	}

	return null;
});

function rootSetSelected(value: boolean) {
	emit('selection', {
		indexKey: props.selection?.indexKey ?? 0,
		keys: value ? props.rows.map((x) => x.id) : [],
		allSelected: value,
	} as ISelection);
}

function updateSelectedRow(mediaId: number, state: boolean) {
	emit('selection', {
		...props.selection,
		keys: state ? [...(props.selection?.keys ?? []), mediaId] : (props.selection?.keys ?? []).filter((x) => x !== mediaId),
		allSelected: false,
	} as ISelection);
}

onMounted(() => {
	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(qTableRef)) {
			Log.error('qTableRef is null');
			return;
		}

		if (!props.scrollDict) {
			Log.error('Could not find scrollDict');
			return;
		}

		// We have to revert to normal title sort otherwise the index will be wrong
		setMediaOverviewSort({ sort: 'asc', field: 'sortTitle' });

		const index = props.scrollDict[letter] ? props.scrollDict[letter] : 0;
		// noinspection TypeScriptValidateTypes
		const element: HTMLElement | null = get(qTableRef)?.querySelector(`[data-scroll-index="${index}"]`) ?? null;
		if (!element) {
			Log.error(`Could not find scroll target element for letter ${letter}`, `[data-scroll-index="${index}"]`);
			return;
		}

		set(scrollTargetElement, element);
		set(autoScrollEnabled, true);

		const elementRect = get(scrollTargetElement)?.getBoundingClientRect();
		// Scroll if not visible
		if ((elementRect?.bottom ?? 0) >= 0 && (elementRect?.top ?? 0) <= window.innerHeight) {
			triggerBoxHighlight(element);
		} else {
			get(scrollTargetElement)?.scrollIntoView({
				block: 'start',
				behavior: 'smooth',
			});
		}
	});
	// Setup stopped scrolling event listener
	useScroll(get(qTableRef), {
		onStop() {
			// Don't highlight if the user scrolls manually
			if (!get(autoScrollEnabled)) {
				return;
			}
			set(autoScrollEnabled, false);
			triggerBoxHighlight(get(scrollTargetElement));
		},
	});
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.media-table {
	// overflow-y: auto;

	&--header,
	&--intersection,
	&--intersection > div {
		height: $media-table-row-height;
	}

	&--content {
		max-height: calc(100vh - $app-bar-height - $media-overview-bar-height - $media-table-row-height);
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
