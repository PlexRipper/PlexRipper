<template>
	{{ selected }}
	<QTreeViewTableRow v-model:model-value="selected" is-header :node="headerNode" :columns="columns" />
	<q-separator />
	<q-tree
		v-model:ticked="ticked"
		class="q-tree-view-table"
		tick-strategy="leaf"
		:nodes="nodes"
		node-key="id"
		no-connectors
		accordion
		:default-expand-all="defaultExpandAll"
		:label-key="labelKey">
		<template #default-header="{ node }">
			<QTreeViewTableRow :columns="columns" :node="node" @action="$emit('action', $event)" />
		</template>
	</q-tree>
</template>

<script setup lang="ts">
import { defineProps, withDefaults, ref, defineEmits } from 'vue';
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

defineEmits<{
	(e: 'update:model-value', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();

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
@import '@/assets/scss/variables.scss';

.q-tree-view-table {
	.q-tree {
		&__node {
			border-bottom-style: solid;
			border-bottom-width: 1px;
			padding-bottom: 0;

			.q-tree__node-header,
			.q-tree__children .q-tree__node-header {
				margin-top: 0;
				border-radius: 0;
			}

			&:last-child {
				border-bottom: none;
			}
		}
	}
}

.body--dark {
	.q-tree-view-table {
		.q-tree__node {
			border-bottom-color: $separator-dark-color;
		}
	}
}

.body--light {
	.q-tree-view-table {
		.q-tree__node {
			border-bottom-color: $separator-color;
		}
	}
}

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
