<template>
	<QSection>
		<template #header>
			{{ t('pages.settings.advanced.download-manager.header') }}
		</template>
		<!--	Max segmented downloads	-->
		<HelpRow help-id="help.settings.advanced.download-manager-section.download-segments">
			<QSlider
				:model-value="settingsStore.downloadManagerSettings.downloadSegments"
				label
				label-always
				:min="1"
				:max="8"
				:step="1"
				@change="settingsStore.downloadManagerSettings.downloadSegments = $event"
			/>
		</HelpRow>
	</QSection>
</template>

<script setup lang="ts">
import { useSettingsStore } from '~/store';

const { t } = useI18n();
const settingsStore = useSettingsStore();
</script>
