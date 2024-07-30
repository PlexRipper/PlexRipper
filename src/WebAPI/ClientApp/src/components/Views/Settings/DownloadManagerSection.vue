<template>
	<QSection>
		<template #header>
			{{ $t('pages.settings.advanced.download-manager.header') }}
		</template>
		<!--	Max segmented downloads	-->
		<HelpRow
			:label="$t('help.settings.advanced.download-manager-section.download-segments.label')"
			:title="$t('help.settings.advanced.download-manager-section.download-segments.title')"
			:text="$t('help.settings.advanced.download-manager-section.download-segments.text')"
		>
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

const settingsStore = useSettingsStore();
</script>
