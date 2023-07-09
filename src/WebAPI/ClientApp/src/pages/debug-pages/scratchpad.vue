<template>
	<q-page>
		<PrimeTreeTable :nodes="nodes" :columns="getDownloadTableColumns" />
	</q-page>
</template>

<script setup lang="ts">
import { TreeNode } from 'primevue/tree/Tree';
import { getDownloadTableColumns } from '@composables/mediaTableColumns';
import { generateDownloadTaskTvShows } from '@factories';
import { DownloadProgressDTO } from '@dto/mainApi';

const nodes = computed((): TreeNode[] => {
	return mapToTreeNodes(
		generateDownloadTaskTvShows({
			plexLibraryId: 1,
			plexServerId: 1,
			config: {
				tvShowDownloadTask: 20,
			},
		}),
	);
});

function mapToTreeNodes(value: DownloadProgressDTO[]): TreeNode[] {
	return value.map((x) => {
		return {
			...x,
			key: x.id,
			data: x,
			label: x.title,
			children: mapToTreeNodes(x.children),
		} as TreeNode;
	});
}
</script>
