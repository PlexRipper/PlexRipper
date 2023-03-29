<template>
	<q-markup-table>
		<thead>
			<tr>
				<th v-for="column in columns" :key="column.field">
					{{ column.label }}
					<q-icon v-if="column.sortable" :name="column.sortDirection === 'asc' ? 'mdi-arrow-up' : 'mdi-arrow-down'" />
				</th>
			</tr>
		</thead>
		<tbody>
			<tr v-for="(row, rowIndex) in nodes" :key="rowIndex">
				<td>
					<q-tree
						v-model:ticked="ticked"
						tick-strategy="leaf"
						:nodes="[row]"
						node-key="id"
						no-connectors
						:default-expand-all="props.defaultExpandAll"
						:label-key="labelKey">
						<template #default-header="{ node }">
							<q-tr>
								<q-td v-for="(column, index) in columns" :key="column.field">
									{{ node[column.field] }}
								</q-td>
							</q-tr>
						</template>
					</q-tree>
				</td>
			</tr>
		</tbody>
	</q-markup-table>
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
		defaultExpandAll: boolean;
	}>(),
	{
		nodes: () => [],
		labelKey: 'label',
	},
);

const selected = ref('');
const ticked = ref([]);
const expanded = ref([]);
</script>
