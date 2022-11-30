<template>
	<v-simple-table class="section-table">
		<tbody>
			<tr>
				<td style="width: 25%">
					<help-icon help-id="help.server-dialog.server-config.download-speed-limit" />
				</td>
				<td>
					<download-limit-input
						:plex-server-id="plexServer.id"
						:download-speed-limit="downloadSpeedLimit"
						@change="updateDownloadLimit"
					/>
				</td>
			</tr>
		</tbody>
	</v-simple-table>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerDTO, PlexServerSettingsModel } from '@dto/mainApi';
import { SettingsService } from '@service';

@Component<ServerConfigTabContent>({})
export default class ServerConfigTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Object as () => PlexServerSettingsModel })
	readonly plexServerSettings!: PlexServerSettingsModel;

	get downloadSpeedLimit(): number {
		return this.plexServerSettings?.downloadSpeedLimit ?? 0;
	}

	updateDownloadLimit(value) {
		if (value < 0) {
			value = 0;
		}
		if (this.plexServerSettings) {
			Log.info(value);
			this.plexServerSettings.downloadSpeedLimit = value;
			// Its copied due to the object containing Vue getters and setters which messes up the store
			useSubscription(
				SettingsService.updateServerSettings(JSON.parse(JSON.stringify(this.plexServerSettings))).subscribe(),
			);
		}
	}
}
</script>
