<template>
	<HelpRow
		v-if="plexServer"
		:label="$t('help.server-dialog.server-config.download-speed-limit.label')"
		:title="$t('help.server-dialog.server-config.download-speed-limit.title')"
		:text="$t('help.server-dialog.server-config.download-speed-limit.text')"
	>
		<DownloadLimitInput
			:download-speed-limit="settingsStore.getServerSettings(plexServer.machineIdentifier)?.downloadSpeedLimit ?? 0"
			@change="settingsStore.updateDownloadLimit(plexServer.machineIdentifier, $event)"
		/>
	</HelpRow>
	<QAlert
		v-else
		type="error"
	>
		{{ $t('components.server-dialog.tabs.server-config.plex-server-was-null') }}
	</QAlert>
</template>

<script setup lang="ts">
import type { PlexServerDTO } from '@dto';
import { useSettingsStore } from '~/store';

const settingsStore = useSettingsStore();

defineProps<{
	plexServer: PlexServerDTO | null;
}>();
</script>
