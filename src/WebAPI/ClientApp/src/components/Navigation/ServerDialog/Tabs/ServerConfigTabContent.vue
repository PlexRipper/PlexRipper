<template>
	<q-row align="center">
		<q-col cols="4">
			<help-icon help-id="help.server-dialog.server-config.download-speed-limit" class="q-mt-sm" />
		</q-col>
		<q-col cols="8">
			<download-limit-input
				v-if="plexServer"
				:plex-server-id="plexServer.id"
				:download-speed-limit="downloadSpeedLimit"
				@change="settingsStore.updateDownloadLimit(plexServer.machineIdentifier, $event)" />
			<span v-else> Plex Server was null </span>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import type { PlexServerDTO, PlexServerSettingsModel } from '@dto/mainApi';
import { useSettingsStore } from '~/store';
const settingsStore = useSettingsStore();

const props = defineProps<{
	plexServer: PlexServerDTO | null;
	plexServerSettings: PlexServerSettingsModel | null;
}>();

const downloadSpeedLimit = computed((): number => {
	return props.plexServerSettings?.downloadSpeedLimit ?? 0;
});
</script>
