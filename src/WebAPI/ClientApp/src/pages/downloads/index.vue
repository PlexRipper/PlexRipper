<template>
	<QPage>
		<!-- Download Toolbar -->
		<DownloadBar />

		<!--	The Download Table	-->
		<QRow
			v-if="downloadStore.getServersWithDownloads.length > 0"
			justify="center">
			<QCol cols="12">
				<q-list>
					<DownloadsTable
						v-for="{ plexServer, downloads } in downloadStore.getServersWithDownloads"
						:key="plexServer.id"
						:download-rows="downloads"
						:plex-server="plexServer"
						@action="commandSwitch($event)" />
				</q-list>
			</QCol>
		</QRow>
		<QRow
			v-else
			justify="center">
			<QCol cols="auto">
				<h2>{{ t('pages.downloads.no-downloads') }}</h2>
			</QCol>
		</QRow>
		<DownloadDetailsDialog :name="dialogName" />
	</QPage>
</template>

<script setup lang="ts">
import type { DownloadProgressDTO } from '@dto';
import { useOpenControlDialog } from '#imports';

const { t } = useI18n();
const downloadStore = useDownloadStore();
const dialogName = 'download-details-dialog';

// region single commands

function commandSwitch({ action, item }: { action: string; item: DownloadProgressDTO }) {
	const ids: string[] = [item.id];

	if (action === 'details') {
		useOpenControlDialog(dialogName, item.id);
		return;
	}

	downloadStore.executeDownloadCommand(action, ids);
}

// endregion
</script>
