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
import { Component, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { ref, Ref } from 'vue';
// eslint-disable-next-line import/named
import { watchDebounced } from '@vueuse/core';
import { SettingsService } from '@service';

@Component
export default class DownloadManagerSection extends Vue {
	downloadSegments: Ref<number> = ref(0);

	mounted(): void {
		watchDebounced(
			this.downloadSegments,
			(value) => {
				SettingsService.updateDownloadManagerSetting('downloadSegments', value);
			},
			{ debounce: 1000, maxWait: 5000 },
		);

		useSubscription(
			SettingsService.getDownloadSegments().subscribe((downloadSegments) => {
				this.downloadSegments.value = downloadSegments;
			}),
		);
	}
}
</script>
