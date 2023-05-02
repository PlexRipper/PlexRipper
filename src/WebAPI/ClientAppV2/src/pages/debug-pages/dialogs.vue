<template>
	<q-page>
		<q-section>
			<template #header> {{ $t('pages.debug.dialogs.header') }}</template>
			<q-markup-table>
				<q-tr>
					<q-td>
						<DebugButton :label="$t('pages.debug.dialogs.buttons.server-dialog')" @click="openServerDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton
							:label="$t('pages.debug.dialogs.buttons.download-confirmation')"
							@click="openDownloadConfirmationDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton :label="$t('pages.debug.dialogs.buttons.help-dialog')" @click="openHelpDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton text-id="add-alert" @click="addAlert" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton
							data-cy="check-server-connections-dialog-button"
							:label="$t('pages.debug.dialogs.buttons.check-server-connections-dialog')"
							@click="openCheckServerConnectionsDialog" />
					</q-td>
				</q-tr>
			</q-markup-table>
			<ServerDialog :name="serverDialogName" />
			<DownloadConfirmation :name="downloadConfirmationName" />
		</q-section>
	</q-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { useOpenControlDialog } from '@composables/event-bus';
import { DownloadConfirmation } from '#components';
import { AlertService, HelpService, MediaService } from '@service';
import { PlexMediaDTO } from '@dto/mainApi';
import { generateDownloadTasks } from '@mock/mock-download-task';

const serverDialogName = 'debugServerDialog';
const downloadConfirmationName = 'debugDownloadConfirmation';

const mediaItem = ref<PlexMediaDTO | null>();
const mediaTableRows = ref<PlexMediaDTO[]>([]);
const downloadRows = generateDownloadTasks(1, { tvShowDownloadTask: 10 });

function openServerDialog(): void {
	useOpenControlDialog(serverDialogName, 1);
}

function openDownloadConfirmationDialog(): void {
	const demo = [
		{
			plexServerId: 1,
			plexLibraryId: 9,
			mediaIds: [24, 25, 26, 27, 28],
			type: 'TvShow',
		},
	];
	useOpenControlDialog(downloadConfirmationName, demo);
}

function openHelpDialog(): void {
	HelpService.openHelpDialog('help.settings.ui.language.language-selection');
}

function openCheckServerConnectionsDialog(): void {
	useOpenControlDialog('checkServerConnectionDialogName');
}

function addAlert(): void {
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
}

onMounted(() => {
	useSubscription(
		MediaService.getTvShowMediaData(32).subscribe((data) => {
			if (data) {
				mediaTableRows.value = [data];
				mediaItem.value = data;
			}
		}),
	);
});
</script>
