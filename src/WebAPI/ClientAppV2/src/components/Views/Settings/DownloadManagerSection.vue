<template>
	<q-section>
		<template #header> {{ $t('pages.settings.advanced.download-manager.header') }}</template>
		<!--	Max segmented downloads	-->
		<q-row>
			<q-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.download-manager-section.download-segments" />
			</q-col>
			<q-col cols="6" align-self="center">
				<QSlider
					label
					label-always
					:model-value="downloadSegments"
					:min="1"
					:max="8"
					:step="1"
					@change="updateSettings('downloadSegments', $event)">
				</QSlider>
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { SettingsService } from '@service';
import { DownloadManagerSettingsDTO } from '@dto/mainApi';

const downloadSegments = ref(0);

const updateSettings = (setting: keyof DownloadManagerSettingsDTO, value: any): void => {
	useSubscription(SettingsService.updateDownloadManagerSetting(setting, value).subscribe());
};

onMounted(() => {
	useSubscription(
		SettingsService.getDownloadSegments().subscribe((value) => {
			set(downloadSegments, value);
		}),
	);
});
</script>
