<template>
	<div class="media-table" data-cy="media-table">
		<MediaTableHeader :columns="mediaTableColumns" selectable class="media-table--header" />
		<div ref="qTableRef" class="media-table--content scroll" data-cy="media-table-scroll">
			<q-intersection
				v-for="(row, index) in rows"
				:key="index"
				class="media-table--intersection highlight-border-box"
				:data-scroll-index="index">
				<MediaTableRow :index="index" :columns="mediaTableColumns" :row="row" selectable @action="onRowAction" />
			</q-intersection>
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { computed, defineEmits, defineProps, ref, withDefaults, onMounted } from 'vue';
import { get, set, useScroll } from '@vueuse/core';
import { setMediaOverviewSort, toDownloadMedia, triggerBoxHighlight, useProcessDownloadCommandBus } from '#imports';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';
import { listenMediaOverviewScrollToCommand } from '@composables/event-bus';

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
	}>(),
	{
		scrollDict: { '#': 0 } as any,
	},
);

const emit = defineEmits<{
	(e: 'selection', payload: ISelection): void;
	(e: 'row-click', payload: PlexMediaSlimDTO): void;
}>();

/**
 * The selected rows cannot be returned as just keys, they need to be the same object as the rows.
 */
const getSelected = computed((): PlexMediaSlimDTO[] => {
	return props.rows.filter((row) => (props.selection?.keys ?? []).includes(row.id));
});

const updateSelected = (selected: PlexMediaSlimDTO[]) => {
	emit('selection', {
		keys: selected.map((x) => x.id) as number[],
		allSelected: selected.length === props.rows.length ? true : selected.length === 0 ? false : null,
		indexKey: 0,
	});
};

function onRowAction({ action, data }: { action: 'download'; data: PlexMediaSlimDTO }) {
	if (action === 'download') {
		const processDownloadCommandBus = useProcessDownloadCommandBus();
		processDownloadCommandBus.emit(toDownloadMedia(data));
	}
}

onMounted(() => {
	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(qTableRef)) {
			Log.error('qTableRef is null');
			return;
		}

	// We have to revert to normal title sort otherwise the index will be wrong
	setMediaOverviewSort({ sort: 'asc', field: 'sortTitle' });

	const index = props.scrollDict[letter];
	// noinspection TypeScriptValidateTypes
	const element: HTMLElement = get(qTableRef)?.querySelector(`[data-scroll-index="${index}"]`);
	if (!element) {
		Log.error(`Could not find scroll target element for letter ${letter}`, `[data-scroll-index="${index}"]`);
		return;
	}

	set(scrollTargetElement, element);
	set(autoScrollEnabled, true);

		const elementRect = get(scrollTargetElement)?.getBoundingClientRect();
		// Scroll if not visible
		if (elementRect?.bottom >= 0 && elementRect?.top <= window.innerHeight) {
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
	overflow-y: auto;

	&--header,
	&--intersection,
	&--intersection > div {
		height: $media-table-row-height;
	}

	&--content {
		max-height: calc(100vh - $app-bar-height - $media-overview-bar-height - $media-table-row-height);
		overflow-x: hidden;
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
