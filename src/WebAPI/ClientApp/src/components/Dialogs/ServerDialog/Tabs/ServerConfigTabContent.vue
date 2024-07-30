<template>
	<HelpRow help-id="help.server-dialog.server-config.download-speed-limit">
		<download-limit-input
			v-if="plexServer"
			:download-speed-limit="settingsStore.getServerSettings(plexServer.machineIdentifier)?.downloadSpeedLimit ?? 0"
			@change="settingsStore.updateDownloadLimit(plexServer.machineIdentifier, $event)"
		/>
		<span v-else> Plex Server was null </span>
	</HelpRow>
</template>

<script setup lang="ts">
import type { PlexServerDTO } from '@dto';
import { useSettingsStore } from '~/store';

const settingsStore = useSettingsStore();

defineProps<{
	plexServer: PlexServerDTO | null;
}>();
</script>
