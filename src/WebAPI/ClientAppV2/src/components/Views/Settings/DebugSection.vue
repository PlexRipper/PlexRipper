<template>
	<q-section>
		<template #header> {{ $t('pages.settings.advanced.debug.header') }}</template>
		<!--	Reset Database	-->
		<q-row>
			<q-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.debug-section" />
			</q-col>
			<q-col cols="4" align-self="center">
				<q-toggle :model-value="debugMode" size="lg" @update:model-value="updateSettings('debugMode', $event)" />
			</q-col>
		</q-row>
	</q-section>
</template>
<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { GeneralSettingsDTO } from '@dto/mainApi';
import { SettingsService } from '@service';

const debugMode = ref(false);

function updateSettings(key: keyof GeneralSettingsDTO, state: boolean): void {
	useSubscription(SettingsService.updateGeneralSettings(key, state).subscribe());
}

onMounted(() => {
	useSubscription(
		SettingsService.getDebugMode().subscribe((value) => {
			set(debugMode, value);
		}),
	);
});
</script>
