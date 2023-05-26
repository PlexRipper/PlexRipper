<template>
	<div :class="{ 'media-table-header-column': true, sorted: sorted }" @click="onClick">
		<span> {{ column.label }} <q-icon v-if="column.sortable" :name="icon" class="header-sort-icon" /></span>
	</div>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { setMediaOverviewSort } from '@composables/event-bus';
import { QTreeViewTableHeader } from '@props';

const sorted = ref<'asc' | 'desc' | boolean>(false);

const props = defineProps<{
	column: QTreeViewTableHeader;
}>();

defineEmits<{
	(e: 'sort'): void;
}>();
const icon = computed(() => {
	if (get(sorted) === 'asc') {
		return 'mdi-arrow-up';
	} else if (get(sorted) === 'desc') {
		return 'mdi-arrow-down';
	} else {
		return 'mdi-arrow-up';
	}
});

function onClick() {
	switch (get(sorted)) {
		case 'asc':
			set(sorted, 'desc' as 'asc' | 'desc' | boolean);
			break;
		case 'desc':
			set(sorted, false);
			break;
		default:
			set(sorted, 'asc' as 'asc' | 'desc' | boolean);
			break;
	}
	setMediaOverviewSort({
		sort: get(sorted) ? get(sorted) : false,
		field: props.column.sortField ?? props.column.field,
	});
}

onBeforeMount(() => {
	set(sorted, props.column?.sortOrder ?? false);
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