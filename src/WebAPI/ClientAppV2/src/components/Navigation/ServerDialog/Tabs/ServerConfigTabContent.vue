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
				@change="updateDownloadLimit($event)" />
			<span v-else> Plex Server was null </span>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';

import { useSubscription } from '@vueuse/rxjs';
import { clone } from 'lodash-es';
import type { PlexServerDTO, PlexServerSettingsModel } from '@dto/mainApi';
import { SettingsService } from '@service';

const props = defineProps<{
	plexServer: PlexServerDTO | null;
	plexServerSettings: PlexServerSettingsModel | null;
}>();

const downloadSpeedLimit = computed((): number => {
	return props.plexServerSettings?.downloadSpeedLimit ?? 0;
});

function updateDownloadLimit(value) {
	if (value < 0) {
		value = 0;
	}
	if (props.plexServerSettings) {
		Log.debug('downloadSpeedLimit', value);
		const settings = clone(props.plexServerSettings);
		settings.downloadSpeedLimit = value;
		// Its copied due to the object containing Vue getters and setters which messes up the store
		useSubscription(SettingsService.updateServerSettings(settings).subscribe());
	}
}
</script>
