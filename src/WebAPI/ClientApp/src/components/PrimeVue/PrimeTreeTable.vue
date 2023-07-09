<template>
	<PTreeTable
		:value="nodes"
		auto-layout
		:selection-keys="selected"
		selection-mode="checkbox"
		class="p-treetable-sm"
		:paginator="true"
		:rows="10"
		scrollable
		scroll-height="flex"
		paginator-position="both"
		:page-link-size="10"
		:row-hover="true"
		:rows-per-page-options="[10, 25, 50, 100]"
		@update:selection-keys="onSelectionChange">
		<PColumn
			v-for="(column, index) in columns"
			:key="index"
			column-key="id"
			:style="{ minWidth: index === 0 ? '20%' : '7%', maxWidth: index === 0 ? 'none' : '100px', height: '55px' }"
			:field="column.field"
			:header="column.label"
			:expander="index === 0">
			<template #header>
				<QCheckbox
					v-if="column.type === 'title'"
					:model-value="headerSelected"
					@update:model-value="$emit('all-selected', $event)" />
			</template>
			<template #body="{ node }: { node: any, data: TreeNode }">
				<PTreeTableRow :column="column" :index="index" :node="node" @action="$emit('action', $event)" />
			</template>
		</PColumn>
	</PTreeTable>
</template>

<script setup lang="ts">
import { TreeNode } from 'primevue/tree/Tree';
import { TreeTableSelectionKeys } from 'primevue/treetable';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';
import IPTreeTableSelectionKeys from '@interfaces/IPTreeTableSelectionKeys';

defineProps<{
	nodes: TreeNode[];
	columns: QTreeViewTableHeader[];
	headerSelected?: boolean | null;
	selected: IPTreeTableSelectionKeys;
	maxSelectionCount?: number;
	notSelectable?: boolean;
}>();

function onSelectionChange(keys: IPTreeTableSelectionKeys) {
	const filtered = Object.fromEntries(
		Object.entries(keys).filter(
			([_, val]: [string, { checked: boolean; partialChecked: boolean }]) => val.checked || val.partialChecked,
		),
	);
	emits('selected', filtered);
}

const emits = defineEmits<{
	(e: 'update:model-value', payload: boolean): void;
	(e: 'selected', payload: TreeTableSelectionKeys): void;
	(e: 'all-selected', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();
</script>
<style lang="scss">
.p-treetable {
	table {
		white-space: nowrap;
		width: 100%;
	}

	.p-treetable-header {
		color: inherit;
		background: transparent;
		border: none;
	}
	.p-treetable-thead > tr > th {
		color: inherit;
		background: transparent;
		border-top: rgba(255, 255, 255, 0.28) 0.13rem solid;
		border-bottom: rgba(255, 255, 255, 0.28) 0.13rem solid;
	}

	.p-treetable-tbody > tr {
		color: inherit;
		background: transparent;
		//outline: 0.15rem solid white;
		border-bottom: rgba(255, 255, 255, 0.28) 0.13rem solid;

		&:focus {
			outline: none;
		}
	}

	.p-checkbox-box {
		&.p-highlight {
			border-color: red;
		}
		.p-checkbox-icon {
			color: white;
		}
	}

	.p-paginator {
		color: inherit;
		background: transparent;
	}
}
</style>
