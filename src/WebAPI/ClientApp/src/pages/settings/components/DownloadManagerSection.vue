<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.header') }} </template>
		<!--	Max segmented downloads	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.download-manager-section.download-segments" />
			</v-col>
			<v-col cols="8" align-self="center">
				<v-slider min="1" max="8" :value="downloadSegments" @input="updateSettings(0, $event)">
					<template #append>
						<p>{{ downloadSegments }}</p>
					</template>
				</v-slider>
			</v-col>
		</v-row>
		<!--	Reset Database	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.reset-db" />
			</v-col>
			<v-col cols="8" align-self="center">
				<p-btn :button-type="dbResetButtonType" text-id="reset-db" @click="resetDatabaseCommand" />
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import SettingsService from '@state/settingsService';
import ButtonType from '@enums/buttonType';
import { resetDatabase } from '@api/settingsApi';

@Component
export default class DownloadManagerSection extends Vue {
	downloadSegments: number = 0;

	dbResetButtonType: ButtonType = ButtonType.Warning;

	updateSettings(index: number, state: any): void {
		SettingsService.updateDownloadManagerSettings({
			downloadSegments: index === 0 ? state : this.downloadSegments,
		});
	}

	resetDatabaseCommand(): void {
		Log.debug('Reset DB');
		this.$subscribeTo(resetDatabase(), () => {});
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
