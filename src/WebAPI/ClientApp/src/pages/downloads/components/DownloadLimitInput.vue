<template>
	<v-col>
		<p-text-field
			:value="downloadSpeedLimit"
			full-width
			hide-details
			single-line
			type="number"
			suffix="kB/s"
			hide-spin-buttons
			@click.prevent
			@change="updateDownloadLimit"
		/>
	</v-col>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { SettingsService } from '@service';
import { map, switchMap } from 'rxjs/operators';
import { PlexServerSettingsModel } from '@dto/mainApi';

@Component<DownloadLimitInput>({
	components: {},
})
export default class DownloadLimitInput extends Vue {
	@Prop({ required: true, type: Number })
	readonly plexServerId!: number;

	plexServerSettings: PlexServerSettingsModel | null = null;

	updateDownloadLimit(value) {
		if (value < 0) {
			value = 0;
		}
		if (this.plexServerSettings) {
			this.plexServerSettings.downloadSpeedLimit = value;
			// Its copied due to the object containing Vue getters and setters which messes up the store
			SettingsService.updateServerSettings(JSON.parse(JSON.stringify(this.plexServerSettings)));
		}
	}

	get downloadSpeedLimit(): number {
		return this.plexServerSettings?.downloadSpeedLimit ?? 0;
	}

	mounted(): void {
		this.$subscribeTo(
			this.$watchAsObservable('plexServerId').pipe(
				map((x: { oldValue: number; newValue: number }) => x.newValue),
				switchMap((id: number) => SettingsService.getServerSettings(id)),
			),
			(value) => {
				this.plexServerSettings = value;
			},
		);
	}
}
</script>
