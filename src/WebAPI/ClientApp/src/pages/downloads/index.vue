<template>
	<QPage>
		<template v-if="downloadStore.getServersWithDownloads.length > 0">
			<!-- Download Toolbar -->
			<DownloadBar />

			<!--	The Download Table	-->
			<QRow
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
			<DownloadDetailsDialog :name="dialogName" />
		</template>
		<QRow
			v-else
			justify="center">
			<QAlert>
				{{ $t('pages.downloads.no-downloads') }}
			</QAlert>
		</QRow>
	</QPage>
</template>

<script setup lang="ts">
import type { DownloadProgressDTO } from '@dto';
import { useOpenControlDialog } from '#imports';

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
