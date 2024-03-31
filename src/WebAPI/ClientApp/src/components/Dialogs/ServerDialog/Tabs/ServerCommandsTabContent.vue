<template>
	<div>
		<HelpRow help-id="help.server-dialog.server-commands.inspect-server">
			<BaseButton :disabled="syncLoading" :loading="inspectLoading" text-id="inspect-server" @click="inspectServer" />
		</HelpRow>
		<HelpRow help-id="help.server-dialog.server-commands.sync-server-libraries">
			<BaseButton
				:disabled="inspectLoading"
				:loading="syncLoading"
				text-id="sync-server-libraries"
				@click="syncServerLibraries" />
		</HelpRow>
	</div>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { ref, onUnmounted } from '#imports';
import type { PlexServerDTO } from '@dto/mainApi';
import { plexServerApi } from '@api';

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
