<template>
	<q-section>
		<template #header>
			{{ t('pages.settings.ui.general-settings.header') }}
		</template>
		<!--	Toggle Animated Background	-->
		<help-row help-id="help.settings.ui.general-settings.toggle-animated-background">
			<q-toggle
				v-model:model-value="settingsStore.generalSettings.disableAnimatedBackground"
				size="lg"
				data-cy="toggle-animated-background" />
		</help-row>
	</q-section>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const { t } = useI18n();
const settingsStore = useSettingsStore();
</script>
