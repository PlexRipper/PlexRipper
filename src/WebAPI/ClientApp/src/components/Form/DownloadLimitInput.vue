<template>
	<QRow
		class="no-wrap"
		justify="between">
		<QCol :cols="8">
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
		<QCol :cols="3">
			<q-input
				v-model:model-value="value"
				type="number"
				debounce="1000"
				suffix="kB/s"
				data-cy="download-speed-limit-input" />
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
const props = defineProps<{
	downloadSpeedLimit: number;
}>();

const emit = defineEmits<{
	(e: 'change', value: number): void;
}>();

const value = computed({
	get: () => {
		return props.downloadSpeedLimit ?? 0;
	},
	set: (value) => {
		if (value == null || value < 0) {
			value = 0;
		}
		emit('change', value);
	},
});
</script>
