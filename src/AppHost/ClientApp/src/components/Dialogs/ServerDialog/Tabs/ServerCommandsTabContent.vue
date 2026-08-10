<template>
	<HelpRow
		disable-responsive
		:label="$t('help.server-dialog.server-commands.inspect-server.label')"
		:title="$t('help.server-dialog.server-commands.inspect-server.title')"
		:text="$t('help.server-dialog.server-commands.inspect-server.text')">
		<BaseButton
			:disabled="syncLoading || deleteLoading"
			:loading="inspectLoading"
			:label="$t('general.commands.inspect-server')"
			@click="inspectServer" />
	</HelpRow>
	<HelpRow
		disable-responsive
		:label="$t('help.server-dialog.server-commands.sync-server-libraries.label')"
		:title="$t('help.server-dialog.server-commands.sync-server-libraries.title')"
		:text="$t('help.server-dialog.server-commands.sync-server-libraries.text')">
		<BaseButton
			:disabled="inspectLoading || deleteLoading"
			:loading="syncLoading"
			:label="$t('general.commands.sync-server-libraries')"
			@click="syncServerLibraries" />
	</HelpRow>
	<HelpRow
		disable-responsive
		:label="$t('help.server-dialog.server-commands.delete-server.label')"
		:title="$t('help.server-dialog.server-commands.delete-server.title')"
		:text="$t('help.server-dialog.server-commands.delete-server.text')">
		<DeleteButton
			:disabled="inspectLoading || syncLoading"
			:loading="deleteLoading"
			:label="$t('general.commands.delete-server')"
			@click="dialogStore.openDialog(DialogType.ServerDeleteConfirmationDialog)" />
	</HelpRow>
	<ConfirmationDialog
		:confirm-loading="deleteLoading"
		:name="DialogType.ServerDeleteConfirmationDialog"
		:title="$t('confirmation.delete-server.title')"
		:text="$t('confirmation.delete-server.text')"
		:warning="$t('confirmation.delete-server.warning')"
		:confirm-label="$t('general.commands.delete-server')"
		class="q-mr-md"
		@confirm="deleteServer" />
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerDTO } from '@dto';
import { DialogType } from '@enums';
import { plexServerApi } from '@api';
import { useDialogStore, useServerStore } from '@store';
import { ref, onUnmounted } from '#imports';

const dialogStore = useDialogStore();
const serverStore = useServerStore();

const props = defineProps<{
	plexServer: PlexServerDTO | null;
	isVisible: boolean;
}>();

const syncLoading = ref(false);
const inspectLoading = ref(false);
const deleteLoading = ref(false);

function syncServerLibraries(): void {
	if (!props.plexServer) {
		return;
	}
	set(syncLoading, true);
	useSubscription(
		plexServerApi
			.syncPlexServerMediaEndpoint(props.plexServer.id, {
				forceLibrarySync: false,
				forceMediaRefresh: false,
			})
			.subscribe({
				next: (result) => {
					set(syncLoading, false);
					if (!result.isSuccess) {
						return;
					}
					dialogStore.closeDialog(DialogType.ServerSettingsDialog);
					dialogStore.openDialog(DialogType.SyncServerMediaDialog);
				},
				error: () => {
					set(syncLoading, false);
				},
			}),
	);
}

function inspectServer(): void {
	if (!props.plexServer) {
		return;
	}
	set(inspectLoading, true);
	useSubscription(
		plexServerApi.queueInspectPlexServerJobEndpoint(props.plexServer.id).subscribe({
			next: () => {
				set(inspectLoading, false);
			},
			error: () => {
				set(inspectLoading, false);
			},
		}),
	);
}

function deleteServer(): void {
	if (!props.plexServer) {
		return;
	}
	set(deleteLoading, true);
	useSubscription(
		serverStore.deleteServer(props.plexServer.id).subscribe({
			next: (result) => {
				set(deleteLoading, false);
				if (!result.isSuccess) {
					return;
				}
				dialogStore.closeDialog(DialogType.ServerDeleteConfirmationDialog);
				dialogStore.closeDialog(DialogType.ServerSettingsDialog);
			},
			error: () => {
				set(deleteLoading, false);
			},
		}),
	);
}

onUnmounted(() => {
	set(syncLoading, false);
	set(inspectLoading, false);
	set(deleteLoading, false);
});
</script>
