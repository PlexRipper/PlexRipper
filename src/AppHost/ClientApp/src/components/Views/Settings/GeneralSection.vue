<template>
	<QSection :header="$t('pages.settings.ui.general-settings.header')">
		<HelpGroup class="q-mt-md">
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
			<!--	Mask Plex Account Names	-->
			<HelpRow
				:label="$t('help.settings.ui.general-settings.mask-plex-accounts.label')"
				:title="$t('help.settings.ui.general-settings.mask-plex-accounts.title')"
				:text="$t('help.settings.ui.general-settings.mask-plex-accounts.text')">
				<q-toggle
					v-model:model-value="settingsStore.debugSettings.maskAccountNames"
					size="lg" />
			</HelpRow>
			<!--	Mask Plex Server Names	-->
			<HelpRow
				:label="$t('help.settings.ui.general-settings.mask-plex-servers.label')"
				:title="$t('help.settings.ui.general-settings.mask-plex-servers.title')"
				:text="$t('help.settings.ui.general-settings.mask-plex-servers.text')">
				<q-toggle
					v-model:model-value="settingsStore.debugSettings.maskServerNames"
					size="lg" />
			</HelpRow>
			<!--	Mask Plex Library Names	-->
			<HelpRow
				:label="$t('help.settings.ui.general-settings.mask-plex-libraries.label')"
				:title="$t('help.settings.ui.general-settings.mask-plex-libraries.title')"
				:text="$t('help.settings.ui.general-settings.mask-plex-libraries.text')">
				<q-toggle
					v-model:model-value="settingsStore.debugSettings.maskLibraryNames"
					size="lg" />
			</HelpRow>
		</HelpGroup>
	</QSection>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();
</script>
