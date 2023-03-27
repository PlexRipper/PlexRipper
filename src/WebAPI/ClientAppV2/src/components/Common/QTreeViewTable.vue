<template>
	<q-table :rows="nodes" :columns="columns" hide-bottom :rows-per-page-options="[0]">
		<template #body="row">
			<q-tr>
				<q-tree
					v-model:ticked="ticked"
					tick-strategy="leaf"
					:nodes="[row.row]"
					node-key="id"
					no-connectors
					:label-key="labelKey" />
			</q-tr>
		</template>
	</q-table>
	<div>
		<pre>
			{{ ticked }}
		</pre>
	</div>
	<!--	<q-hierarchy :data="nodes" :columns="columns" />-->
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, withDefaults, ref } from 'vue';
import { QTreeViewTableProps } from '~/types/props/q-treeview-table/QTreeViewTableProps';

defineOptions({
	inheritAttrs: false,
});

const props = withDefaults(defineProps<QTreeViewTableProps>(), {
	nodes: () => [],
	labelKey: 'label',
});

const selected = ref('');
const ticked = ref([]);
const expanded = ref([]);
</script>
