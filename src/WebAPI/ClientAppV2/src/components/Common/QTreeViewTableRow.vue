<template>
	<q-row
		justify="between"
		align="center"
		no-wrap
		:class="{ 'q-tree-view-table-row': true, 'q-tree-view-table-row--header': isHeader }"
		class="q-mb-xs">
		<!--	Title Column	-->
		<q-col class="q-ml-sm">
			<q-row align="center" justify="start">
				<q-col v-if="isHeader" cols="auto" class="q-ml-md q-pl-sm">
					<q-checkbox dense :model-value="selected" @update:model-value="$emit('selected', $event)" />
				</q-col>
				<q-col align-self="start" cols="auto" class="header-column">
					<q-media-type-icon v-if="node['mediaType']" :media-type="node['mediaType']" />
					{{ node[columns[0].field] }}
				</q-col>
			</q-row>
		</q-col>
		<!--	Rest of the Columns	-->
		<q-col cols="auto" :style="{ 'max-width': `${getContainerWidth}px !important` }">
			<q-row align="center" justify="end" no-wrap>
				<template v-for="(column, i) in columns.slice(1)" :key="column.field">
					<!--	Table Header Row	-->
					<q-col v-if="isHeader" cols="auto" :text-align="'center'" :width="column?.width ?? 0">
						<span>
							{{ node[column.field] }}
						</span>
					</q-col>
					<!--	Rest of the Columns	-->
					<q-col v-else cols="auto" class="table-column" :width="column?.width ?? 0" :text-align="column.align">
						<!-- Duration format -->
						<template v-if="column['type'] === 'duration'">
							<QDuration short :value="node[column.field]" />
						</template>
						<!-- Date format -->
						<template v-else-if="column['type'] === 'date'">
							<QDateTime short-date :text="node[column.field]" />
						</template>
						<!-- Filesize format -->
						<template v-else-if="column['type'] === 'file-size'">
							<QFileSize :size="node[column.field]" />
						</template>
						<!-- Filesize format -->
						<template v-else-if="column['type'] === 'file-speed'">
							<QFileSize :size="node[column.field]" speed />
						</template>
						<!-- Percentage -->
						<template v-else-if="column['type'] === 'percentage'">
							<QProgressBar :value="node[column.field]" />
						</template>
						<!-- Actions -->
						<template v-else-if="column['type'] === 'actions'">
							<q-row justify="start" no-wrap>
								<q-col cols="auto">
									<!-- Item Actions -->
									<BaseButton
										v-for="(action, y) in node[column.field]"
										:key="`${i}-${y}`"
										icon-only
										dense
										flat
										square
										:icon="Convert.buttonTypeToIcon(action)"
										@click.stop="
											$emit('action', {
												action: action,
												data: node,
											})
										" />
								</q-col>
							</q-row>
						</template>
						<!-- Default format -->
						<template v-else>
							{{ node[column.field] }}
						</template>
					</q-col>
				</template>
			</q-row>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from 'vue';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';
import Convert from '@class/Convert';

defineOptions({
	inheritAttrs: false,
});

const props = defineProps<{
	selected?: boolean | null;
	isHeader?: boolean;
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
