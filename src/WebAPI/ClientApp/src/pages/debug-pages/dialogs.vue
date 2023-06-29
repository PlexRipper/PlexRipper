<template>
	<q-page>
		<q-section>
			<template #header> {{ t('pages.debug.dialogs.header') }}</template>
			<q-markup-table>
				<q-tr>
					<q-td>
						<DebugButton :label="t('pages.debug.dialogs.buttons.server-dialog')" @click="openServerDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton
							:label="t('pages.debug.dialogs.buttons.download-confirmation')"
							@click="openDownloadConfirmationDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton :label="t('pages.debug.dialogs.buttons.help-dialog')" @click="openHelpDialog" />
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
							:label="t('pages.debug.dialogs.buttons.check-server-connections-dialog')"
							@click="openCheckServerConnectionsDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton
							data-cy="open-verification-dialog-button"
							:label="t('pages.debug.dialogs.buttons.verification-dialog')"
							@click="openVerificationDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton
							data-cy="directory-browser-dialog-button"
							:label="t('pages.debug.dialogs.buttons.directory-browser')"
							@click="openDirectoryBrowserDialog" />
					</q-td>
				</q-tr>
			</q-markup-table>
			<ServerDialog :name="serverDialogName" />
			<DownloadConfirmation :name="downloadConfirmationName" />
			<AccountVerificationCodeDialog :name="verificationCodeDialogName" :account="account" />
			<DirectoryBrowser :name="directoryBrowserName" />
		</q-section>
	</q-page>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import { useOpenControlDialog } from '@composables/event-bus';
import { DownloadConfirmation } from '#components';
import { MediaService } from '@service';
import { PlexAccountDTO, PlexMediaSlimDTO } from '@dto/mainApi';
import { generateDefaultFolderPaths, generatePlexAccount } from '@factories';
import { useAlertStore, useHelpStore } from '~/store';

const { t } = useI18n();
const helpStore = useHelpStore();
const alertStore = useAlertStore();

const serverDialogName = 'debugServerDialog';
const downloadConfirmationName = 'debugDownloadConfirmation';
const checkServerConnectionDialogName = 'checkServerConnectionDialogName';
const verificationCodeDialogName = 'verificationCodeDialogName';
const directoryBrowserName = 'TestDirectoryBrowser';

const mediaItem = ref<PlexMediaSlimDTO | null>();
const mediaTableRows = ref<PlexMediaSlimDTO[]>([]);
const account = ref<PlexAccountDTO>(
	generatePlexAccount({
		id: 1,
		plexLibraries: [],
		plexServers: [],
	}),
);

const folderPath = generateDefaultFolderPaths({})[0];

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
	helpStore.openHelpDialog('help.settings.ui.language.language-selection');
}

function openCheckServerConnectionsDialog(): void {
	useOpenControlDialog(checkServerConnectionDialogName);
}

function openVerificationDialog(): void {
	useOpenControlDialog(verificationCodeDialogName);
}

function openDirectoryBrowserDialog(): void {
	useOpenControlDialog(directoryBrowserName, folderPath);
}

function addAlert(): void {
	alertStore.showAlert({ id: 0, title: 'Alert Title 1', text: 'random alert' });
	alertStore.showAlert({ id: 0, title: 'Alert Title 2', text: 'random alert' });
}

onMounted(() => {
	useSubscription(
		MediaService.getTvShowMediaData(32).subscribe((data) => {
			if (data) {
				set(mediaTableRows, [data]);
				set(mediaItem, data);
			}
		}),
	);
});
</script>
