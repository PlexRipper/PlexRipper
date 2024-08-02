<template>
	<QSection>
		<template #header>
			{{ $t('pages.settings.ui.general-settings.header') }}
		</template>
		<!--	Toggle Animated Background	-->
		<HelpRow
			:label="$t('help.settings.ui.general-settings.toggle-animated-background.label')"
			:title="$t('help.settings.ui.general-settings.toggle-animated-background.title')"
			:text="$t('help.settings.ui.general-settings.toggle-animated-background.text')">
			<q-toggle
				v-model:model-value="settingsStore.generalSettings.disableAnimatedBackground"
				size="lg"
				data-cy="toggle-animated-background" />
		</HelpRow>
	</QSection>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();
</script>
