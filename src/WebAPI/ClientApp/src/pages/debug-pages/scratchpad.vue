<template>
	<q-page>
		<DownloadsTable
			:plex-server="plexServer"
			:download-rows="downloadStore.getDownloadsByServerId(plexServer.id)"
			@action="commandSwitch($event)" />
		<download-details-dialog :name="dialogName" />
	</q-page>
</template>

<script setup lang="ts">
import { generateDownloadProgressTvShows, generatePlexServer } from '@factories';
import { DownloadProgressDTO } from '@dto/mainApi';
import { useOpenControlDialog } from '@composables/event-bus';
const downloadStore = useDownloadStore();
const dialogName = 'download-details-dialog';

const plexServer = generatePlexServer({
	id: 1,
});

function commandSwitch({ action, item }: { action: string; item: DownloadProgressDTO }) {
	const ids: number[] = [item.id];

	if (action === 'details') {
		useOpenControlDialog(dialogName, item.id);
		return;
	}

	downloadStore.executeDownloadCommand(action, ids);
}

const downloadTasks = generateDownloadProgressTvShows({
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
