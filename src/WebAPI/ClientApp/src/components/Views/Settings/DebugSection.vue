<template>
	<q-section>
		<template #header>
			{{ t('pages.settings.advanced.debug.header') }}
		</template>
		<!--	Toggle Debug Mode	-->
		<HelpRow help-id="help.settings.advanced.debug-section.debug-mode">
			<q-toggle
				v-model:model-value="settingsStore.debugSettings.debugModeEnabled"
				size="lg"
			/>
		</HelpRow>
		<!--	Mask Plex Server Names	-->
		<HelpRow help-id="help.settings.advanced.debug-section.mask-plex-servers">
			<q-toggle
				v-model:model-value="settingsStore.debugSettings.maskServerNames"
				size="lg"
			/>
		</HelpRow>
		<!--	Mask Plex Library Names	-->
		<HelpRow help-id="help.settings.advanced.debug-section.mask-plex-libraries">
			<q-toggle
				v-model:model-value="settingsStore.debugSettings.maskLibraryNames"
				size="lg"
			/>
		</HelpRow>
	</q-section>
</template>

<script setup lang="ts">
import { useSettingsStore } from '~/store';

const { t } = useI18n();
const settingsStore = useSettingsStore();
</script>
