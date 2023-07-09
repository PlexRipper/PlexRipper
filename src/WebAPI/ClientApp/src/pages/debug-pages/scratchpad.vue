<template>
	<q-page>
		<print expanded>
			{{ downloadStore.getSelectedDownloadTasks(plexServerId) }}
		</print>
		<PrimeTreeTable
			:nodes="nodes"
			:columns="getDownloadTableColumns"
			:selected="downloadStore.getSelectedDownloadTasks(plexServerId)"
			:header-selected="downloadStore.getHeaderSelection(plexServerId)"
			:max-selection-count="downloadStore.getDownloadSelection(plexServerId)?.maxSelectionCount"
			@selected="downloadStore.updateSelectedDownloadTasks(plexServerId, $event)"
			@all-selected="downloadStore.setAllSelectedDownloadTasks(plexServerId, $event)" />
	</q-page>
</template>

<script setup lang="ts">
import { TreeNode } from 'primevue/tree/Tree';
import { getDownloadTableColumns } from '@composables/mediaTableColumns';
import { generateDownloadTaskTvShows } from '@factories';
import { DownloadProgressDTO } from '@dto/mainApi';
const plexServerId = 1;
const downloadStore = useDownloadStore();
const nodes = computed((): TreeNode[] => {
	return mapToTreeNodes(downloadStore.getDownloadsByServerId(plexServerId));
});

const downloadTasks = generateDownloadTaskTvShows({
	plexLibraryId: 1,
	plexServerId,
	config: {
		tvShowDownloadTask: 500,
		seasonDownloadTask: 2,
		episodeDownloadTask: 2,
	},
});

downloadStore.updateServerDownloadProgress({
	id: plexServerId,
	downloads: downloadTasks,
	downloadableTasksCount: 10,
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
</script>
