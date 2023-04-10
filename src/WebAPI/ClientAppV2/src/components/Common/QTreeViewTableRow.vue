<template>
	<q-row
		justify="between"
		align="center"
		:class="{ 'q-tree-view-table-row': true, 'q-tree-view-table-row--header': isHeader }"
		class="q-mb-xs">
		<q-col v-if="isHeader" cols="auto" class="q-ml-md q-pl-sm">
			<q-checkbox dense :model-value="modelValue" @update:model-value="$emit('update:model-value', $event)" />
		</q-col>
		<!--	Title Column	-->
		<q-col class="q-ml-sm">
			<q-row align="center">
				<q-col align-self="start" cols="auto" class="header-column">
					<q-media-type-icon v-if="node['mediaType']" :media-type="node['mediaType']" />
					{{ node[columns[0].field] }}
				</q-col>
			</q-row>
		</q-col>
		<!--	Rest of the Columns	-->
		<q-col cols="9" :style="{ 'max-width': `${getContainerWidth}px !important` }">
			<q-row align="center" justify="between">
				<template v-for="(column, i) in columns.slice(1)" :key="column.field">
					<q-col cols="grow" class="table-column">
						<template v-if="isHeader">
							{{ node[column.field] }}
						</template>
						<!-- Duration format -->
						<template v-else-if="column['type'] === 'duration'">
							<QDuration :value="node[column.field]" />
						</template>
						<!-- Date format -->
						<template v-else-if="column['type'] === 'date'">
							<QDateTime :text="node[column.field]" />
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
	modelValue?: boolean;
	isHeader?: boolean;
	node: QTreeViewTableItem;
	columns: QTreeViewTableHeader[];
}>();

defineEmits<{
	(e: 'update:model-value', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();

const getContainerWidth = computed(() => {
	return props.columns.reduce((acc, column) => {
		return acc + (column?.width ?? 100);
	}, 0);
});
</script>
<style lang="scss">
.q-tree-view-table-row {
	height: 30px;
	text-align: center;

	&--header {
		font-weight: bold;
		height: 40px;
	}
}
</style>
