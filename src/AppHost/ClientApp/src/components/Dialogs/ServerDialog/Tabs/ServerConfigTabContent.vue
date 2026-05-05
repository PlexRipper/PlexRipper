<template>
	<HelpGroup v-if="plexServer">
		<!-- Enabled -->
		<HelpRow
			disable-responsive
			label="Enabled"
			title="Enable this Plex server"
			text="Disabled servers are excluded from queries/syncs and hidden from normal views.">
			<QToggle
				size="lg"
				:model-value="serverStore.getServer(plexServer.id)!.isEnabled"
				@update:model-value="onServerEnabledChanged" />
		</HelpRow>
		<!-- Owned -->
		<HelpRow
			disable-responsive
			label="Owned"
			title="Mark server as owned"
			text="Owned servers are excluded from Sonarr/Radarr source candidate lists.">
			<QToggle
				size="lg"
				:model-value="serverStore.getServer(plexServer.id)!.owned"
				@update:model-value="onServerOwnedChanged" />
		</HelpRow>

		<!-- Stream Downloader -->
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

		<!-- Download Speed Limit -->
		<HelpRow
			disable-responsive
			:label="$t('help.server-dialog.server-config.download-speed-limit.label')"
			:title="$t('help.server-dialog.server-config.download-speed-limit.title')"
			:text="$t('help.server-dialog.server-config.download-speed-limit.text')">
			<DownloadLimitInput :machine-identifier="plexServer.machineIdentifier" />
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
import { useServerStore, useSettingsStore } from '@store';

const props = defineProps<{
	plexServer: PlexServerDTO | null;
}>();

const settingsStore = useSettingsStore();
const serverStore = useServerStore();

const allowStreamDownloader = computed(
	() => settingsStore.getServerSettings(props.plexServer?.machineIdentifier)?.allowStreamDownloader ?? false,
);

function onAllowStreamDownloaderChanged(value: boolean) {
	if (props.plexServer) {
		settingsStore.updateAllowStreamDownloader(props.plexServer.machineIdentifier, value);
	}
}

function onServerEnabledChanged(value: boolean) {
	if (!props.plexServer) {
		return;
	}

	useSubscription(serverStore.setServerEnabled(props.plexServer.id, value).subscribe());
}

function onServerOwnedChanged(value: boolean) {
	if (!props.plexServer) {
		return;
	}

	useSubscription(serverStore.setServerOwned(props.plexServer.id, value).subscribe());
}
</script>
