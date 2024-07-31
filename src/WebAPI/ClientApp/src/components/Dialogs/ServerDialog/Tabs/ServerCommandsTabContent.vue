<template>
	<div>
		<HelpRow
			:label="$t('help.server-dialog.server-commands.inspect-server.label')"
			:title="$t('help.server-dialog.server-commands.inspect-server.title')"
			:text="$t('help.server-dialog.server-commands.inspect-server.text')">
			<BaseButton
				:disabled="syncLoading"
				:loading="inspectLoading"
				:label="$t('general.commands.inspect-server')"
				@click="inspectServer" />
		</HelpRow>
		<HelpRow
			:label="$t('help.server-dialog.server-commands.sync-server-libraries.label')"
			:title="$t('help.server-dialog.server-commands.sync-server-libraries.title')"
			:text="$t('help.server-dialog.server-commands.sync-server-libraries.text')">
			<BaseButton
				:disabled="inspectLoading"
				:loading="syncLoading"
				:label="$t('general.commands.sync-server-libraries')"
				@click="syncServerLibraries" />
		</HelpRow>
	</div>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import type { PlexServerDTO } from '@dto';
import { plexServerApi } from '@api';
import { ref, onUnmounted } from '#imports';

const props = defineProps<{
	plexServer: PlexServerDTO | null;
	isVisible: boolean;
}>();

const syncLoading = ref(false);
const inspectLoading = ref(false);

function syncServerLibraries(): void {
	if (!props.plexServer) {
		return;
	}
	set(syncLoading, true);
	useSubscription(
		plexServerApi
			.queueSyncPlexServerJobEndpoint(props.plexServer.id, {
				forceSync: true,
			})
			.subscribe(() => {
				set(syncLoading, false);
			}),
	);
}

function inspectServer(): void {
	if (!props.plexServer) {
		return;
	}
	set(inspectLoading, true);
	useSubscription(
		plexServerApi.queueInspectPlexServerJobEndpoint(props.plexServer.id).subscribe(() => {
			set(inspectLoading, false);
		}),
	);
}

onUnmounted(() => {
	set(syncLoading, false);
	set(inspectLoading, false);
});
</script>
