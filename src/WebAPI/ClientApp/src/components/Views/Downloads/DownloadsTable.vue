<template>
	<q-expansion-item default-opened class="background-sm q-ma-md">
		<template #header>
			<q-row justify="between" align="center">
				<!-- Download Server Settings -->
				<q-col cols="auto" style="white-space: nowrap">
					<QCheckbox
						style="display: inline-block"
						:model-value="downloadStore.getHeaderSelection(plexServer.id)"
						@update:model-value="downloadStore.setAllSelectedDownloadTasks(plexServer.id, $event)" />
					<server-download-status v-if="false" style="display: inline-block" />
				</q-col>
				<q-col> </q-col>
				<!-- Download Server Title -->
				<q-col cols="auto">
					<span class="title">{{ plexServer.name }}</span>
				</q-col>
				<q-col class="q-py-none"></q-col>
			</q-row>
		</template>
		<template #default>
			<PrimeTreeTable
				:nodes="nodes"
				:columns="getDownloadTableColumns"
				:selected="downloadStore.getSelectedDownloadTasks(plexServer.id)"
				:max-selection-count="downloadStore.getDownloadSelection(plexServer.id)?.maxSelectionCount"
				@action="tableAction($event)"
				@selected="downloadStore.updateSelectedDownloadTasks(plexServer.id, $event)" />
		</template>
	</q-expansion-item>
</template>

<script setup lang="ts">
import Log from 'consola';
import { TreeNode } from 'primevue/tree';
import { DownloadProgressDTO, PlexServerDTO } from '@dto/mainApi';
import { QTreeViewTableItem } from '@props';
import { getDownloadTableColumns } from '#imports';
import ISelection from '@interfaces/ISelection';

const downloadStore = useDownloadStore();

const props = defineProps<{
	loading?: boolean;
	plexServer: PlexServerDTO;
	downloadRows: DownloadProgressDTO[];
}>();

const emit = defineEmits<{
	(e: 'action', payload: { action: string; item: DownloadProgressDTO }): void;
	(e: 'selected', payload: ISelection): void;
}>();

const nodes = computed((): TreeNode[] => {
	// TODO: Move this to the back-end to increase performance
	return mapToTreeNodes(downloadStore.getDownloadsByServerId(props.plexServer.id));
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
</script>
