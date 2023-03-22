<template>
	<q-row class="no-wrap" justify="between">
		<q-col :cols="8">
			<q-slider
				:model-value="value"
				:min="0"
				:step="500"
				:max="100000"
				snap
				label
				style="margin-top: 24px"
				@pan="mouseEvent = $event"
				@change="changeValue"
				@update:model-value="updateDownloadLimit" />
		</q-col>
		<q-col :cols="3">
			<q-input :model-value="value" type="number" suffix="kB/s" @update="changeValue" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, computed } from 'vue';

const props = defineProps<{
	plexServerId: number;
	downloadSpeedLimit: number;
}>();

const emit = defineEmits<{
	(e: 'change', value: number): void;
}>();

const mouseEvent = ref('');
const sliderValue = ref(0);

const downloadSpeedLimit = computed(() => {
	return props.downloadSpeedLimit ?? 0;
});

const value = computed(() => {
	if (mouseEvent.value === 'start') {
		return sliderValue.value;
	}
	return downloadSpeedLimit.value;
});

function updateDownloadLimit(value: number): void {
	if (value < 0) {
		value = 0;
	}
	sliderValue.value = value;
}

function changeValue(value: number): void {
	sliderValue.value = value;
	emit('change', value);
}
</script>
