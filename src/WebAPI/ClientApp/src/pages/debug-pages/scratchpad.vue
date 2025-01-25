<template>
	<QPage>
		<DownloadsTable
			:plex-server="plexServer"
			:download-rows="downloadStore.getDownloadsByServerId(plexServer.id)"
			@action="commandSwitch($event)" />
		<DownloadDetailsDialog :name="dialogName" />
		<BaseButton
			label="Reload Stores"
			@click="onAction" />
	</QPage>
</template>

<script setup lang="ts">
import { generateDownloadProgressTvShows, generatePlexServer, Seed } from '@factories';
import { DownloadActions, type DownloadProgressDTO } from '@dto';
import { useDownloadStore, useGlobalStore, useDialogStore } from '@store';
import { useSubscription } from '@vueuse/rxjs';

const globalStore = useGlobalStore();
const downloadStore = useDownloadStore();
const dialogStore = useDialogStore();
const dialogName = 'download-details-dialog';

const plexServer = generatePlexServer({
	id: 1,
});

function commandSwitch({ action, item }: { action: DownloadActions; item: DownloadProgressDTO }) {
	const ids: string[] = [item.id];

	if (action === DownloadActions.Details) {
		dialogStore.openDownloadTaskDetailsDialog(item.id);
		return;
	}

	downloadStore.executeDownloadCommand(action, ids);
}

function onAction() {
	globalStore.$reset();
	useSubscription(globalStore.setup().subscribe());
}

const downloadTasks = generateDownloadProgressTvShows({
	plexLibraryId: 1,
	plexServerId: plexServer.id,
	config: {
		tvShowDownloadTask: 500,
		seasonDownloadTask: 2,
		episodeDownloadTask: 2,
	},
	seed: new Seed(1),
});

downloadStore.updateServerDownloadProgress({
	id: plexServer.id,
	downloads: downloadTasks,
	downloadableTasksCount: 10,
});
</script>
