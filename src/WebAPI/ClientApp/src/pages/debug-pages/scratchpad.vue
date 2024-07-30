<template>
	<QPage>
		<DownloadsTable
			:plex-server="plexServer"
			:download-rows="downloadStore.getDownloadsByServerId(plexServer.id)"
			@action="commandSwitch($event)"
		/>
		<DownloadDetailsDialog :name="dialogName" />
	</QPage>
</template>

<script setup lang="ts">
import { generateDownloadProgressTvShows, generatePlexServer } from '@factories';
import type { DownloadProgressDTO } from '@dto';
import { useOpenControlDialog } from '@composables/event-bus';

const downloadStore = useDownloadStore();
const dialogName = 'download-details-dialog';

const plexServer = generatePlexServer({
	id: 1,
});

function commandSwitch({ action, item }: { action: string; item: DownloadProgressDTO }) {
	const ids: string[] = [item.id];

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
