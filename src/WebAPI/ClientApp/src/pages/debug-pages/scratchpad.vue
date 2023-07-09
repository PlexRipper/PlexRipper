<template>
	<q-page>
		<DownloadsTable :plex-server="plexServer" :download-rows="downloadStore.getDownloadsByServerId(plexServer.id)" />
	</q-page>
</template>

<script setup lang="ts">
import { generateDownloadTaskTvShows, generatePlexServer } from '@factories';
const downloadStore = useDownloadStore();

const plexServer = generatePlexServer({
	id: 1,
});

const downloadTasks = generateDownloadTaskTvShows({
	plexLibraryId: 1,
	plexServerId: plexServer.id,
	config: {
		tvShowDownloadTask: 500,
		seasonDownloadTask: 2,
		episodeDownloadTask: 2,
	},
});

downloadStore.updateServerDownloadProgress({
	id: plexServer.id,
	downloads: downloadTasks,
	downloadableTasksCount: 10,
});
</script>
