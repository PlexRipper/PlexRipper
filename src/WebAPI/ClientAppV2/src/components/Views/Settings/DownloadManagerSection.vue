<template>
	<p-section>
		<template #header> {{ $t('pages.settings.advanced.download-manager.header') }}</template>
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
import { Component, Vue, Watch } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { debounce, interval, Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { SettingsService } from '@service';

@Component
export default class DownloadManagerSection extends Vue {
	downloadSegments: number = 0;

	downloadSegmentSubject: Subject<number> = new Subject<number>();

	@Watch('downloadSegments')
	onDownloadSegments(newValue) {
		this.downloadSegmentSubject.next(newValue);
	}

	mounted(): void {
		useSubscription(
			this.downloadSegmentSubject
				.asObservable()
				.pipe(
					debounce(() => interval(1000)),
					switchMap((value) => SettingsService.updateDownloadManagerSetting('downloadSegments', value)),
				)
				.subscribe(),
		);

		useSubscription(
			SettingsService.getDownloadSegments().subscribe((downloadSegments) => {
				this.downloadSegments = downloadSegments;
			}),
		);
	}
}
</script>
