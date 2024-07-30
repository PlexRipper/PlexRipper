<template>
	<QRow
		v-if="node"
		justify="between"
		align="center"
		no-wrap
		:class="{ 'q-tree-view-table-row': true, 'q-tree-view-table-row--header': isHeader }"
		class="q-mb-xs">
		<!--	Title Column	-->
		<QCol class="q-ml-sm">
			<QRow
				align="center"
				justify="start">
				<QCol
					v-if="isHeader && selectable"
					cols="auto"
					class="q-ml-md q-pl-sm">
					<q-checkbox
						dense
						:model-value="selected"
						@update:model-value="$emit('selected', $event)" />
				</QCol>
				<QCol
					v-if="columns[0]?.field"
					align-self="start"
					cols="auto"
					class="header-column">
					<QMediaTypeIcon
						v-if="node['mediaType']"
						:media-type="node['mediaType']" />
					<span :data-cy="`column-${columns[0]?.field}-${node.id}`">
						{{ node[columns[0].field ?? ''] ?? 'unknown' }}
					</span>
				</QCol>
			</QRow>
		</QCol>
		<!--	Rest of the Columns	-->
		<QCol
			cols="auto"
			:style="{ 'max-width': `${getContainerWidth}px !important` }">
			<QRow
				align="center"
				justify="end"
				no-wrap>
				<template
					v-for="(column, i) in columns.slice(1)"
					:key="column.field">
					<!--	Table Header Row	-->
					<QCol
						v-if="isHeader"
						cols="auto"
						:text-align="'center'"
						:width="column?.width ?? 0">
						<span :data-cy="`column-${column.field}-${node.id}`">
							{{ node[column.field] }}
						</span>
					</QCol>
					<!--	Rest of the Columns	-->
					<QCol
						v-else
						cols="auto"
						class="table-column"
						:width="column?.width ?? 0"
						:text-align="column.align">
						<!-- Duration format -->
						<template v-if="column['type'] === 'duration'">
							<QDuration
								short
								:data-cy="`column-${column.field}-${node.id}`"
								:value="node[column.field]" />
						</template>
						<!-- Date format -->
						<template v-else-if="column['type'] === 'date'">
							<QDateTime
								short-date
								:data-cy="`column-${column.field}-${node.id}`"
								:text="node[column.field]" />
						</template>
						<!-- Filesize format -->
						<template v-else-if="column['type'] === 'file-size'">
							<QFileSize
								:data-cy="`column-${column.field}-${node.id}`"
								:size="node[column.field]" />
						</template>
						<!-- File Speed format -->
						<template v-else-if="column['type'] === 'file-speed'">
							<QFileSize
								:data-cy="`column-${column.field}-${node.id}`"
								:size="node[column.field]"
								speed />
						</template>
						<!-- Percentage -->
						<template v-else-if="column['type'] === 'percentage'">
							<QProgressBar
								:data-cy="`column-${column.field}-${node.id}`"
								:value="node[column.field]" />
						</template>
						<!-- Actions -->
						<template v-else-if="column['type'] === 'actions'">
							<QRow
								justify="start"
								no-wrap>
								<QCol cols="auto">
									<!-- Item Actions -->
									<IconSquareButton
										v-for="(action, y) in node[column.field]"
										:key="`${i}-${y}`"
										dense
										:cy="`column-${column.field}-${node.id}`"
										:icon="Convert.buttonTypeToIcon(action)"
										@click.stop="
											$emit('action', {
												action: action,
												data: node,
											})
										" />
								</QCol>
							</QRow>
						</template>
						<!-- Default format -->
						<template v-else>
							<span :data-cy="`column-${column.field}-${node.id}`">
								{{ node[column.field] }}
							</span>
						</template>
					</QCol>
				</template>
			</QRow>
		</QCol>
	</QRow>
	<QRow v-else>
		<QCol>{{ t('components.q-tree-view-table-row.invalid-node') }}</QCol>
	</QRow>
</template>

<script setup lang="ts">
import type { QTreeViewTableHeader, QTreeViewTableItem } from '@props';
import Convert from '@class/Convert';

defineOptions({
	inheritAttrs: false,
});
const { t } = useI18n();

const props = defineProps<{
	selected?: boolean | null;
	isHeader?: boolean;
	selectable?: boolean;
	node: QTreeViewTableItem;
	columns: QTreeViewTableHeader[];
}>();

defineEmits<{
	(e: 'selected', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();

const getContainerWidth = computed(() => {
	return props.columns.slice(1).reduce((acc, column) => {
		return acc + (column?.width ?? 120);
	}, 0);
});
</script>

<style lang="scss">
.q-tree-view-table-row {
	height: 30px;

	.header-column {
		display: inline-block;
		text-overflow: ellipsis;
		white-space: nowrap;
		overflow: hidden;
	}

	.table-column {
		padding: 0 8px;
	}

	&--header {
		font-weight: bold;
		height: 40px;

		.header-column {
			margin-left: 8px;
			font-weight: bold;
			text-align: center;
		}
	}
}
</style>
