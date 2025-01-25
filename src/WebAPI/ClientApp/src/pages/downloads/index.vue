<template>
	<QPage>
		<template v-if="downloadStore.getServersWithDownloads.length > 0">
			<!-- Download Toolbar -->
			<DownloadBar />
			<QScroll class="page-content-minus-download-bar">
				<!--	The Download Table	-->
				<QRow
					justify="center"
					class="q-mb-lg">
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
			</QScroll>
			<DownloadDetailsDialog />
		</template>
		<QRow
			v-else
			justify="center"
			class="q-pa-lg">
			<QAlert>
				{{ $t('pages.downloads.no-downloads') }}
			</QAlert>
		</QRow>
	</QPage>
</template>

<script setup lang="ts">
import { DownloadActions, type DownloadProgressDTO } from '@dto';
import { useDownloadStore, useDialogStore } from '@store';

const dialogStore = useDialogStore();
const downloadStore = useDownloadStore();

function commandSwitch({ action, item }: { action: DownloadActions; item: DownloadProgressDTO }) {
	const ids: string[] = [item.id];

	if (action === DownloadActions.Details) {
		dialogStore.openDownloadTaskDetailsDialog(item.id);
		return;
	}

	downloadStore.executeDownloadCommand(action, ids);
}
</script>
