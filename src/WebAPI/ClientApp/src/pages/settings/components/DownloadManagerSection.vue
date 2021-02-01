<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.header') }} </template>
		<!--	Max segmented downloads	-->
		<v-row>
			<v-col cols="4">
				<help-icon help-id="help.download-manager-section.download-segments" />
			</v-col>
			<v-col cols="8">
				<v-slider min="1" max="8" :value="downloadSegments" @input="updateSettings(0, $event)">
					<template #append>
						<p>{{ downloadSegments }}</p>
					</template>
				</v-slider>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import SettingsService from '@state/settingsService';

@Component
export default class DownloadManagerSection extends Vue {
	downloadSegments: number = 0;

	updateSettings(index: number, state: any): void {
		SettingsService.updateDownloadManagerSettings({
			downloadSegments: index === 0 ? state : this.downloadSegments,
		});
	}

	mounted(): void {
		this.$subscribeTo(SettingsService.getDownloadManagerSettings(), (downloadManagerSettings) => {
			if (downloadManagerSettings) {
				this.downloadSegments = downloadManagerSettings.downloadSegments;
			}
		});
	}
}
</script>
