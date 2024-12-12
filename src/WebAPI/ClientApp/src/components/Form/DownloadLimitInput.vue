<template>
	<QRow
		class="no-wrap"
		justify="between">
		<QCol>
			<q-slider
				v-model:model-value="value"
				:min="0"
				:step="500"
				:max="100000"
				snap
				label
				style="margin-top: 24px"
				data-cy="download-speed-limit-slider" />
		</QCol>
		<QCol
			cols="auto"
			class="q-ml-lg">
			<InputNumber
				v-model="value"
				debounce="1000"
				:min="0"
				suffix=" kB/s"
				data-cy="download-speed-limit-input" />
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();

const props = defineProps<{
	machineIdentifier: string;
}>();

const value = computed({
	get: () => {
		return settingsStore.getServerSettings(props.machineIdentifier)?.downloadSpeedLimit ?? 0;
	},
	set: (value) => {
		if (value == null || value < 0) {
			value = 0;
		}
		settingsStore.updateDownloadLimit(props.machineIdentifier, value);
	},
});
</script>
