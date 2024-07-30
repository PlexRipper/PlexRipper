<template>
	<q-page>
		<!-- Download Toolbar -->
		<download-bar />

		<!--	The Download Table	-->
		<q-row
			v-if="downloadStore.getServersWithDownloads.length > 0"
			justify="center"
		>
			<q-col cols="12">
				<q-list>
					<downloads-table
						v-for="{ plexServer, downloads } in downloadStore.getServersWithDownloads"
						:key="plexServer.id"
						:download-rows="downloads"
						:plex-server="plexServer"
						@action="commandSwitch($event)"
					/>
				</q-list>
			</q-col>
		</q-row>
		<q-row
			v-else
			justify="center"
		>
			<q-col cols="auto">
				<h2>{{ t('pages.downloads.no-downloads') }}</h2>
			</q-col>
		</q-row>
		<download-details-dialog :name="dialogName" />
	</q-page>
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
