<template>
	<!--	<q-row justify="end">-->
	<!--		<q-col cols="auto" class="q-mx-md">-->
	<!--			<q-checkbox />-->
	<!--		</q-col>-->
	<!--		<q-col-->
	<!--			v-for="(column, i) in columns"-->
	<!--			:key="column.field"-->
	<!--			:align-self="i === 0 ? 'start' : 'none'"-->
	<!--			:cols="i === 0 ? 'auto' : '1'">-->
	<!--			{{ column.label }}-->
	<!--			<q-icon v-if="column.sortable" :name="column.sortDirection === 'asc' ? 'mdi-arrow-up' : 'mdi-arrow-down'" />-->
	<!--		</q-col>-->
	<!--	</q-row>-->
	{{ selected }}
	<QTreeViewTableRow v-model:model-value="selected" is-header :node="headerNode" :columns="columns" />
	<q-tree
		v-model:ticked="ticked"
		tick-strategy="leaf"
		:nodes="nodes"
		node-key="id"
		no-connectors
		accordion
		:default-expand-all="defaultExpandAll"
		:label-key="labelKey">
		<template #default-header="{ node }">
			<QTreeViewTableRow :columns="columns" :node="node" />
		</template>
	</q-tree>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, withDefaults, ref } from 'vue';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';

defineOptions({
	inheritAttrs: false,
});

const props = withDefaults(
	defineProps<{
		nodes: QTreeViewTableItem[];
		columns: QTreeViewTableHeader[];
		labelKey?: string;
		defaultExpandAll?: boolean;
	}>(),
	{
		nodes: () => [],
		labelKey: 'label',
	},
);

const selected = ref<boolean>(false);
const ticked = ref([]);
const expanded = ref([]);

const headerNode = computed((): QTreeViewTableItem => {
	const node = {} as QTreeViewTableItem;

	props.columns.forEach((column) => {
		node[column.field] = column.label;
	});

	return node;
});
</script>
<style lang="scss">
.header-column {
	//min-width: 100px;
	//max-width: 400px;
	display: inline-block;
	text-overflow: ellipsis;
	white-space: nowrap;
	overflow: hidden;
}

.table-column {
	//min-width: 150px;
}
</style>
