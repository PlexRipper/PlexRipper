<template>
	<TreeTable
		:value="nodes"
		auto-layout
		:selection-keys="selected"
		selection-mode="checkbox"
		:paginator="true"
		:rows="10"
		scrollable
		scroll-height="flex"
		paginator-position="both"
		:page-link-size="10"
		:row-hover="true"
		size="small"
		:rows-per-page-options="[10, 25, 50, 100]"
		@update:selection-keys="onSelectionChange">
		<Column
			field="title"
			header="Title"
			expander>
			<template #header>
				<QCheckbox
					:model-value="headerSelected"
					@update:model-value="$emit('all-selected', $event)" />
			</template>
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QMediaTypeIcon
					v-if="node.mediaType"
					:size="26"
					:media-type="node.mediaType"
					class="q-mr-md" />
				<span :data-cy="`column-${column.field}-${node.id}`">{{ node.title }}</span>
			</template>
		</Column>
		<Column
			field="status"
			header="Status"
			style="max-width: 10rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<span :data-cy="`column-${column.field}-${node.id}`">
					{{ node.status }}
				</span>
			</template>
		</Column>
		<Column
			field="dataReceived"
			header="Received"
			style="max-width: 10rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QFileSize
					:data-cy="`column-${column.field}-${node.id}`"
					:size="node.dataReceived" />
			</template>
		</Column>
		<Column
			field="dataTotal"
			header="Size"
			style="max-width: 10rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QFileSize
					:data-cy="`column-${column.field}-${node.id}`"
					:size="node.dataTotal" />
			</template>
		</Column>
		<Column
			field="downloadSpeed"
			header="Speed"
			style="max-width: 10rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QFileSize
					:data-cy="`column-${column.field}-${node.id}`"
					:size="node.downloadSpeed"
					speed />
			</template>
		</Column>
		<Column
			field="timeRemaining"
			header="ETA"
			style="max-width: 10rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QDuration
					short
					:data-cy="`column-${column.field}-${node.id}`"
					:value="node.timeRemaining" />
			</template>
		</Column>
		<Column
			field="percentage"
			header="Percentage"
			style="max-width: 10rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QProgressBar
					:data-cy="`column-${column.field}-${node.id}`"
					:value="node.percentage" />
			</template>
		</Column>
		<Column
			field="actions"
			header="Actions"
			style="max-width: 15rem">
			<template #body="{ node, column }: { node: IDownloadTableNode; column: any }">
				<QRow
					justify="start"
					no-wrap>
					<QCol cols="auto">
						<!-- Item Actions -->
						<IconSquareButton
							v-for="(action, y) in node.actions"
							:key="`${node.id}-${y}`"
							dense
							:cy="`column-${column.field}-${action}-${node.id}`"
							:icon="Convert.buttonTypeToIcon(action as ButtonType)"
							@click.stop="
								$emit('action', {
									action: action,
									data: node,
								})
							" />
					</QCol>
				</QRow>
			</template>
		</Column>
	</TreeTable>
</template>

<script setup lang="ts">
import type { TreeTableSelectionKeys } from 'primevue/treetable';
import type { QTreeViewTableHeader } from '@props';
import type IPTreeTableSelectionKeys from '@interfaces/IPTreeTableSelectionKeys';
import type { IDownloadTableNode } from '@interfaces';
import Convert from '@class/Convert';
import type ButtonType from '@enums/buttonType';

defineProps<{
	nodes: IDownloadTableNode[];
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
	(e: 'selected', payload: TreeTableSelectionKeys): void;
	(e: 'update:model-value' | 'all-selected', payload: boolean): void;
	(e: 'action', payload: { action: string; data: IDownloadTableNode }): void;
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

//this creates a pseudochild of the button the size of the first anscestor with "relative" size
button.p-treetable-toggler.p-link::before {
	content: '';
	display: block;
	position: absolute;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
}
//this element originally had "relative" size, overwriting it allow the pseudochild to be sized relative to a later anscestor
.p-treetable-toggler {
	position: static;
}
// this element contains the full row, by making it relative the pseudochild can size itself based on this
.p-treetable .p-treetable-tbody > tr {
	position: relative;
}
</style>
