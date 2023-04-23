<template>
	<QTreeViewTableRow
		:selected="isRootSelected"
		class="q-ma-sm"
		is-header
		:node="headerNode"
		:columns="columns"
		@selected="onSelected($event ? get(getAllLeafIds) : [])" />
	<q-separator />
	<QTree
		ref="qTreeViewTableTreeRef"
		:ticked="selected"
		class="q-tree-view-table q-ma-sm"
		:tick-strategy="isSelectable ? 'leaf' : 'none'"
		:nodes="nodes"
		node-key="id"
		:no-connectors="!connectors"
		:accordion="false"
		:default-expand-all="defaultExpandAll"
		:label-key="labelKey"
		@update:ticked="onSelected">
		<template #default-header="{ node }">
			<QTreeViewTableRow :columns="columns" :selectable="isSelectable" :node="node" @action="$emit('action', $event)" />
		</template>
	</QTree>
</template>

<script setup lang="ts">
import { defineProps, withDefaults, defineEmits } from 'vue';
import { flatMapDeep } from 'lodash-es';
import { QTree } from 'quasar';
import { get } from '@vueuse/core';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';

defineOptions({
	inheritAttrs: false,
});

interface SelectionCheck {
	id: number;
	level: number;
	childIds?: number[];
	children?: SelectionCheck[];
}

const props = withDefaults(
	defineProps<{
		nodes: QTreeViewTableItem[];
		columns: QTreeViewTableHeader[];
		labelKey?: string;
		defaultExpandAll?: boolean;
		selected?: number[] | null;
		connectors?: boolean;
	}>(),
	{
		nodes: () => [],
		labelKey: 'label',
		selected: null,
		connectors: false,
	},
);

const emits = defineEmits<{
	(e: 'update:model-value', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
	(e: 'selected', payload: number[]): void;
	/**
	 * Emitted when the user selects a group of items, e.g. by selecting the root node.
	 * The payload is an aggregation of ids of the selected items.
	 * When all the episodes of a season are selected, then payload will only contain the id the season.
	 * @param e
	 * @param payload
	 */
	(e: 'aggregate-selected', payload: number[]): void;
}>();

const isSelectable = computed((): boolean => {
	return props.selected !== null;
});

const qTreeViewTableTreeRef = ref<InstanceType<typeof QTree> | null>(null);

/**
 * Create a node to be used for the Header row of the QTreeViewTable, the same row component is used to keep the layout consistent.
 */
const headerNode = computed((): QTreeViewTableItem => {
	const node = {} as QTreeViewTableItem;

	props.columns.forEach((column) => {
		node[column.field] = column.label;
	});

	return node;
});

// region Selection
const isRootSelected = computed((): boolean | null => {
	if (!props.selected || !props.selected.length) {
		return false;
	}
	return getAllLeafIds.value.length === props.selected.length ? true : null;
});

/**
 * Get all outermost leaf nodes from the tree.
 * E.g. In a tvShow => season => episode structure, this will return only the episodes.
 */
const getAllLeafIds = computed((): number[] => {
	const getLeafs = (x) => {
		if (!x.children || !x.children.length) {
			return x;
		}
		return [x, flatMapDeep(x.children, getLeafs)];
	};

	return flatMapDeep(props.nodes, getLeafs)
		.filter((x) => x.children?.length === 0)
		.map((x) => x.id);
});

/**
 * Create a tree of selection nodes used to determine which parent nodes should be selected when a child node is selected.
 */
const getSelectionTree = computed((): SelectionCheck[] => {
	function getChildren(nodes: QTreeViewTableItem[], level: number): SelectionCheck[] {
		return nodes.map((node) => {
			const result: SelectionCheck = {
				id: node.id,
				level,
			};
			if (node.children && node.children.length) {
				const children = getChildren(node.children, level + 1);

				result.childIds = children.map((x) => x.id);
				if (children.every((x) => x.childIds && x.childIds.length)) {
					result.children = children;
				}
			}

			return result;
		});
	}

	return getChildren(props.nodes, 0);
});

/**
 * Get a flat array of all selection nodes and sort Desc by the depth level.
 */
const flatSelectionTree = computed((): SelectionCheck[] => {
	const flattenSelectionTree = (x) => {
		if (!x.children || !x.children.length) {
			return x;
		}
		return [x, flatMapDeep(x.children, flattenSelectionTree)];
	};

	return flatMapDeep(get(getSelectionTree), flattenSelectionTree).sort((a, b) => b.level - a.level);
});

/**
 * Match the set of ids of selected items to the selection tree and return the ids of the parent nodes.
 * Check if all Episodes of a Season are selected, then return the id of the Season.
 * @param keys
 */
function getSelectionParents(keys: number[]): number[] {
	const selectionTree = get(flatSelectionTree);
	let selected = keys;

	for (const selection of selectionTree) {
		if (!selection || !selection.childIds || !selection.childIds.length) {
			continue;
		}
		const allSelected = selection.childIds.every((x) => selected.includes(x));
		if (allSelected) {
			selected = selected.filter((x) => !selection.childIds?.includes(x));
			selected.push(selection.id);
		}
	}

	return selected;
}

const onSelected = (keys: number[]) => {
	emits('selected', keys);
	emits('aggregate-selected', getSelectionParents(keys));
};

// endregion
</script>
