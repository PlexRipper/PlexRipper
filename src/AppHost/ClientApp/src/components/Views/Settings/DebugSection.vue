<template>
	<QSection>
		<template #header>
			{{ t('pages.settings.advanced.debug.header') }}
		</template>
		<HelpGroup>
			<!--	Toggle Debug Mode	-->
			<HelpRow
				:label="$t('help.settings.advanced.debug-section.debug-mode.label')"
				:title="$t('help.settings.advanced.debug-section.debug-mode.title')"
				:text="$t('help.settings.advanced.debug-section.debug-mode.text')">
				<q-toggle
					v-model:model-value="settingsStore.debugSettings.debugModeEnabled"
					size="lg" />
			</HelpRow>
		</HelpGroup>
	</QSection>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const { t } = useI18n();
const settingsStore = useSettingsStore();
</script>
