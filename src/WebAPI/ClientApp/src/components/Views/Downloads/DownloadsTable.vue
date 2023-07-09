<template>
	<PrimeTreeTable
		:nodes="nodes"
		:columns="getDownloadTableColumns"
		:selected="downloadStore.getSelectedDownloadTasks(serverId)"
		:header-selected="downloadStore.getHeaderSelection(serverId)"
		:max-selection-count="downloadStore.getDownloadSelection(serverId)?.maxSelectionCount"
		@selected="downloadStore.updateSelectedDownloadTasks(serverId, $event)"
		@all-selected="downloadStore.setAllSelectedDownloadTasks(serverId, $event)">
	</PrimeTreeTable>
</template>

<script setup lang="ts">
import Log from 'consola';
import { TreeNode } from 'primevue/tree';
import { DownloadProgressDTO } from '@dto/mainApi';
import { QTreeViewTableItem } from '@props';
import { getDownloadTableColumns } from '#imports';
import ISelection from '@interfaces/ISelection';

const downloadStore = useDownloadStore();

const props = defineProps<{
	loading?: boolean;
	serverId: number;
	downloadRows: DownloadProgressDTO[];
}>();

const emit = defineEmits<{
	(e: 'action', payload: { action: string; item: DownloadProgressDTO }): void;
	(e: 'selected', payload: ISelection): void;
	(e: 'aggregate-selected', payload: ISelection): void;
}>();

const nodes = computed((): TreeNode[] => {
	// TODO: Move this to the back-end to increase performance
	return mapToTreeNodes(downloadStore.getDownloadsByServerId(props.serverId));
});

function mapToTreeNodes(value: DownloadProgressDTO[]): TreeNode[] {
	return value.map((x) => {
		return {
			...x,
			key: `${x.mediaType}-${x.id}`,
			label: x.title,
			children: mapToTreeNodes(x.children),
		} as TreeNode;
	});
}

function tableAction(payload: { action: string; data: QTreeViewTableItem }) {
	Log.info('command', payload);
	emit('action', {
		action: payload.action,
		item: payload.data as DownloadProgressDTO,
	});
}

function onSelected(selected: number[]) {
	Log.info('selected', selected);
	emit('selected', {
		indexKey: props.serverId,
		keys: selected,
		allSelected: false,
	});
}

function onAggregateSelected(selected: number[]) {
	Log.info('aggregate-selected', selected);
	emit('aggregate-selected', {
		indexKey: props.serverId,
		keys: selected,
		allSelected: false,
	});
}
</script>
