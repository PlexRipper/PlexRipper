<template>
	<q-tree-view-table
		:nodes="getData"
		:selected="selected.keys"
		:columns="getDownloadTableColumns"
		@action="tableAction($event)"
		@selected="onSelected($event)"
		@aggregate-selected="onAggregateSelected($event)" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { DownloadProgressDTO } from '@dto/mainApi';
import { QTreeViewTableItem } from '@props';
import { getDownloadTableColumns } from '#imports';
import ISelection from '@interfaces/ISelection';

const props = defineProps<{
	loading?: boolean;
	serverId: number;
	selected: ISelection;
	downloadRows: DownloadProgressDTO[];
}>();

const emit = defineEmits<{
	(e: 'action', payload: { action: string; item: DownloadProgressDTO }): void;
	(e: 'selected', payload: ISelection): void;
	(e: 'aggregate-selected', payload: ISelection): void;
}>();

const getData = computed((): QTreeViewTableItem[] => {
	return props.downloadRows as QTreeViewTableItem[];
});

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
