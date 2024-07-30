<template>
	<div
		:class="{ 'media-table-header-column': true, 'sorted': sorted }"
		@click="onClick"
	>
		<span> {{ column.label }} <q-icon
			v-if="column.sortable"
			:name="icon"
			class="header-sort-icon"
		/></span>
	</div>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { IMediaOverviewSort } from '@composables/event-bus';
import type { QTreeViewTableHeader } from '@props';
import { useMediaOverviewStore } from '~/store';

const mediaOverviewStore = useMediaOverviewStore();

const props = defineProps<{
	column: QTreeViewTableHeader;
}>();

const sorted = ref<IMediaOverviewSort>({
	sort: props.column.sortOrder ?? 'no-sort',
	field: props.column.sortField ?? props.column.field,
});

defineEmits<{
	(e: 'sort'): void;
}>();

const icon = computed(() => {
	switch (get(sorted).sort) {
		case 'asc':
			return 'mdi-arrow-up';
		case 'desc':
			return 'mdi-arrow-down';
		case 'no-sort':
			return '';
		default:
			return 'mdi-arrow-up';
	}
});

function onClick() {
	const newSort: IMediaOverviewSort = {
		sort: get(sorted)?.sort ?? 'no-sort',
		field: props.column.sortField ?? props.column.field,
	};
	switch (newSort.sort) {
		case 'asc':
			newSort.sort = 'desc';
			break;
		case 'desc':
			newSort.sort = 'asc';
			break;
		case 'no-sort':
			newSort.sort = 'asc';
			break;
		default:
			newSort.sort = 'no-sort';
			break;
	}
	set(sorted, newSort);
	mediaOverviewStore.sortMedia(get(sorted));
}

onBeforeMount(() => {
	set(sorted, {
		field: props.column.sortField ?? props.column.field,
		sort: 'asc',
	});
});
</script>

<style lang="scss">
.media-table-header-column {
	white-space: nowrap;
	font-weight: bold;

	.header-sort-icon {
		opacity: 0;
		transition: transform 0.3s cubic-bezier(0.25, 0.8, 0.5, 1);
	}

	&:hover,
	&.sorted {
		cursor: pointer;

		.header-sort-icon {
			opacity: 1;
		}
	}
}
</style>
