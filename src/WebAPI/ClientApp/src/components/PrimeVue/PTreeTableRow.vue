<template>
	<template v-if="column['type'] === 'title'">
		<q-media-type-icon v-if="node['mediaType']" :media-type="node['mediaType']" class="q-mr-sm" />
		<span :data-cy="`column-${column.field}-${node.id}`">{{ node[column.field] }}</span>
	</template>
	<!-- Duration format -->
	<template v-else-if="column['type'] === 'duration'">
		<QDuration short :node-cy="`column-${column.field}-${node.id}`" :value="node[column.field]" />
	</template>
	<!-- Date format -->
	<template v-else-if="column['type'] === 'date'">
		<QDateTime short-date :node-cy="`column-${column.field}-${node.id}`" :text="node[column.field]" />
	</template>
	<!-- Filesize format -->
	<template v-else-if="column['type'] === 'file-size'">
		<QFileSize :node-cy="`column-${column.field}-${node.id}`" :size="node[column.field]" />
	</template>
	<!-- File Speed format -->
	<template v-else-if="column['type'] === 'file-speed'">
		<QFileSize :node-cy="`column-${column.field}-${node.id}`" :size="node[column.field]" speed />
	</template>
	<!-- Percentage -->
	<template v-else-if="column['type'] === 'percentage'">
		<QProgressBar :node-cy="`column-${column.field}-${node.id}`" :value="node[column.field]" />
	</template>
	<!-- Actions -->
	<template v-else-if="column['type'] === 'actions'">
		<q-row justify="start" no-wrap>
			<q-col cols="auto">
				<!-- Item Actions -->
				<IconSquareButton
					v-for="(action, y) in node[column.field]"
					:key="`${index}-${y}`"
					dense
					:cy="`column-${column.field}-${node.id}`"
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
		<span :data-cy="`column-${column.field}-${node.id}`">
			{{ node[column.field] }}
		</span>
	</template>
</template>

<script setup lang="ts">
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';
import Convert from '@class/Convert';

defineProps<{
	column: QTreeViewTableHeader;
	node: any;
	index: number;
	selected?: boolean | null;
	selectable?: boolean;
}>();

defineEmits<{
	(e: 'selected', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();
</script>
