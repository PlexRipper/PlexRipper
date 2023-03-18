<template>
	<table class="section-table">
		<tbody>
		<tr>
			<td style="width: 25%">
				<help-icon help-id="help.server-dialog.server-config.download-speed-limit"/>
			</td>
			<td v-if="plexServer">
				<download-limit-input
					:plex-server-id="plexServer.id"
					:download-speed-limit="downloadSpeedLimit"
					@change="updateDownloadLimit"
				/>
			</td>
			<td>
				<span> Plex Server was null </span>
			</td>
		</tr>
		</tbody>
	</table>
</template>

<script setup lang="ts">
import Log from 'consola';
import {computed} from '#imports'

import {useSubscription} from '@vueuse/rxjs';
import type {PlexServerDTO, PlexServerSettingsModel} from '@dto/mainApi';
import {SettingsService} from '@service';

const props = defineProps<{
	plexServer: PlexServerDTO;
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
		Log.info(value);
		props.plexServerSettings.downloadSpeedLimit = value;
		// Its copied due to the object containing Vue getters and setters which messes up the store
		useSubscription(
			SettingsService.updateServerSettings(JSON.parse(JSON.stringify(props.plexServerSettings))).subscribe(),
		);
	}
}
</script>
