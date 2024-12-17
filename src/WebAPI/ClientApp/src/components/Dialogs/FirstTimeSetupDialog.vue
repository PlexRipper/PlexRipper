<template>
	<QCardDialog
		:name="DialogType.FirstTimeSetupDialog"
		width="700px">
		<template #title>
			{{ $t('components.first-time-setup-dialog.header') }}
		</template>
		<template #default>
			<QText
				class="q-mb-lg"
				:value="$t('components.first-time-setup-dialog.setup-question')" />
		</template>
		<template #actions="{ close }">
			<NavigationSkipSetupButton
				@click="skipSetup(close)" />

			<GoToButton
				:label="$t('general.commands.go-to-setup-page')"
				to="/setup"

				color="positive" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { DialogType } from '@enums';
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();

function skipSetup(close: () => void) {
	Log.info('Setup process skipped');
	settingsStore.generalSettings.firstTimeSetup = false;
	close();
}
</script>
