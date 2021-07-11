<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.download-manager.header') }} </template>
		<!--	Max segmented downloads	-->
		<v-row>
			<v-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.download-manager-section.download-segments" />
			</v-col>
			<v-col cols="8" align-self="center">
				<v-slider v-model="downloadSegments" min="1" max="8" dense style="height: 36px">
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
import { SettingsService } from '@state';
import { debounce, distinctUntilChanged, map } from 'rxjs/operators';
import { timer } from 'rxjs';

@Component
export default class DownloadManagerSection extends Vue {
	downloadSegments: number = 0;

	updateSettings(index: number, state: any): void {
		SettingsService.updateDownloadManagerSettings({
			downloadSegments: index === 0 ? state : this.downloadSegments,
		});
	}

	mounted(): void {
		this.$subscribeTo(
			this.$watchAsObservable('downloadSegments').pipe(
				map((x: { oldValue: number; newValue: number }) => x.newValue),
				debounce(() => timer(1000)),
				distinctUntilChanged(),
			),
			(value) => {
				this.updateSettings(0, value);
			},
		);

		this.$subscribeTo(SettingsService.getDownloadManagerSettings(), (downloadManagerSettings) => {
			if (downloadManagerSettings) {
				this.downloadSegments = downloadManagerSettings.downloadSegments;
			}
		});
	}
}
</script>
