<template>
	<HelpGroup v-if="plexServer">
		<HelpRow
			disable-responsive
			:label="$t('help.server-dialog.server-config.download-speed-limit.label')"
			:title="$t('help.server-dialog.server-config.download-speed-limit.title')"
			:text="$t('help.server-dialog.server-config.download-speed-limit.text')">
			<DownloadLimitInput :machine-identifier="plexServer.machineIdentifier" />
		</HelpRow>
		<HelpRow
			disable-responsive
			:label="$t('help.server-dialog.server-config.allow-stream-downloader.label')"
			:title="$t('help.server-dialog.server-config.allow-stream-downloader.title')"
			:text="$t('help.server-dialog.server-config.allow-stream-downloader.text')">
			<QToggle
				size="lg"
				:model-value="allowStreamDownloader"
				@update:model-value="onAllowStreamDownloaderChanged" />
		</HelpRow>
	</HelpGroup>

	<QAlert
		v-else
		type="error">
		{{ $t('components.server-dialog.tabs.server-config.plex-server-was-null') }}
	</QAlert>
</template>

<script setup lang="ts">
import type { PlexServerDTO } from '@dto';
import { useSettingsStore } from '@store';

const props = defineProps<{
	plexServer: PlexServerDTO | null;
}>();

const settingsStore = useSettingsStore();

const allowStreamDownloader = computed(
	() => settingsStore.getServerSettings(props.plexServer?.machineIdentifier)?.allowStreamDownloader ?? false,
);

function onAllowStreamDownloaderChanged(value: boolean) {
	if (props.plexServer) {
		settingsStore.updateAllowStreamDownloader(props.plexServer.machineIdentifier, value);
	}
}
</script>
