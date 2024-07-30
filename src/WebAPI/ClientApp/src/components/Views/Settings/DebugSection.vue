<template>
	<QSection>
		<template #header>
			{{ t('pages.settings.advanced.debug.header') }}
		</template>
		<!--	Toggle Debug Mode	-->
		<HelpRow
			:label="$t('help.settings.advanced.debug-section.debug-mode.label')"
			:title="$t('help.settings.advanced.debug-section.debug-mode.title')"
			:text="$t('help.settings.advanced.debug-section.debug-mode.text')"
		>
			<q-toggle
				v-model:model-value="settingsStore.debugSettings.debugModeEnabled"
				size="lg"
			/>
		</HelpRow>
		<!--	Mask Plex Server Names	-->
		<HelpRow
			:label="$t('help.settings.advanced.debug-section.mask-plex-servers.label')"
			:title="$t('help.settings.advanced.debug-section.mask-plex-servers.title')"
			:text="$t('help.settings.advanced.debug-section.mask-plex-servers.text')"
		>
			<q-toggle
				v-model:model-value="settingsStore.debugSettings.maskServerNames"
				size="lg"
			/>
		</HelpRow>
		<!--	Mask Plex Library Names	-->
		<HelpRow
			:label="$t('help.settings.advanced.debug-section.mask-plex-libraries.label')"
			:title="$t('help.settings.advanced.debug-section.mask-plex-libraries.title')"
			:text="$t('help.settings.advanced.debug-section.mask-plex-libraries.text')"
		>
			<q-toggle
				v-model:model-value="settingsStore.debugSettings.maskLibraryNames"
				size="lg"
			/>
		</HelpRow>
	</QSection>
</template>

<script setup lang="ts">
import { useSettingsStore } from '~/store';

const { t } = useI18n();
const settingsStore = useSettingsStore();
</script>
