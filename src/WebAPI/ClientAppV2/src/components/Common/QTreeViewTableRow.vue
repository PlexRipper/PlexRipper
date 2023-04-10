<template>
	<q-row justify="between" align="center" :class="{ 'q-tree-view-table-row': true, 'q-tree-view-table-row--header': isHeader }">
		<q-col v-if="isHeader" cols="auto" class="q-ml-md q-pl-sm">
			<q-checkbox dense :model-value="modelValue" @update:model-value="$emit('update:model-value', $event)" />
		</q-col>
		<q-col class="q-ml-md">
			<q-row align="center">
				<q-col align-self="start" cols="auto" class="header-column">
					{{ node[columns[0].field] }}
				</q-col>
			</q-row>
		</q-col>
		<q-col cols="9" :style="{ 'max-width': `${getContainerWidth}px !important` }">
			<q-row justify="between">
				<template v-for="(column, i) in columns.slice(1)" :key="column.field">
					<q-col cols="grow" class="table-column">
						{{ node[column.field] }}
					</q-col>
				</template>
			</q-row>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from 'vue';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';

defineOptions({
	inheritAttrs: false,
});

const props = defineProps<{
	modelValue?: boolean;
	isHeader?: boolean;
	node: QTreeViewTableItem;
	columns: QTreeViewTableHeader[];
}>();

defineEmits<{ (e: 'update:model-value', payload: boolean): void }>();

const getContainerWidth = computed(() => {
	return props.columns.reduce((acc, column) => {
		return acc + (column?.width ?? 100);
	}, 0);
});
</script>
<style lang="scss">
.q-tree-view-table-row {
	height: 30px;

	&--header {
		font-weight: bold;
	}
}
</style>
