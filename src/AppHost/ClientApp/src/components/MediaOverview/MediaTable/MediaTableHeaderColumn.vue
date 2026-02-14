<template>
	<div
		:class="{ 'media-table-header-column': true, 'sorted': sorted }"
		@click="onClick">
		<span> {{ column.label }} <q-icon
			v-if="column.sortable"
			:name="icon"
			class="header-sort-icon" /></span>
	</div>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { IMediaOverviewSort } from '@composables/event-bus';
import { MediaSortField, SortDirection } from '@enums';
import type { QTreeViewTableHeader } from '@props';
import { useMediaOverviewStore } from '@store';

const mediaOverviewStore = useMediaOverviewStore();

const props = defineProps<{
	column: QTreeViewTableHeader;
}>();

const sorted = ref<IMediaOverviewSort>({
	sort: props.column.sortOrder ?? SortDirection.NoSort,
	field: props.column.sortField ?? MediaSortField.Title,
});

defineEmits<{
	(e: 'sort'): void;
}>();

const icon = computed(() => {
	switch (get(sorted).sort) {
		case SortDirection.Asc:
			return 'mdi-arrow-up';
		case SortDirection.Desc:
			return 'mdi-arrow-down';
		case SortDirection.NoSort:
			return '';
		default:
			return 'mdi-arrow-up';
	}
});

function onClick() {
	const newSort: IMediaOverviewSort = {
		sort: get(sorted)?.sort ?? SortDirection.NoSort,
		field: (props.column.sortField ?? props.column.field) as MediaSortField,
	};
	switch (newSort.sort) {
		case SortDirection.Asc:
			newSort.sort = SortDirection.Desc;
			break;
		case SortDirection.Desc:
			newSort.sort = SortDirection.Asc;
			break;
		case SortDirection.NoSort:
			newSort.sort = SortDirection.Asc;
			break;
		default:
			newSort.sort = SortDirection.NoSort;
			break;
	}
	set(sorted, newSort);
	mediaOverviewStore.sortMedia(get(sorted));
}

onBeforeMount(() => {
	set(sorted, {
		field: (props.column.sortField ?? props.column.field) as MediaSortField,
		sort: SortDirection.Asc,
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
