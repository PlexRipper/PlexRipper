<template>
	<q-row class="no-wrap">
		<q-col :cols="8">
			<q-slider
				:model-value="sliderValue"
				@change="updateDownloadLimit"
				:min="0"
				:step="500"
				:max="10000"
				snap
				label
				style="margin-top: 8px"

			/>
		</q-col>
		<q-col>
			<q-input
				:model-value="sliderValue"
				type="number"
				suffix="kB/s"
				@update="updateDownloadLimit"
			/>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">

const props = defineProps<{
	plexServerId: number;
	downloadSpeedLimit: number;
}>();

const emit = defineEmits<{
	(e: 'change', value: number): void;
}>()

const mouseEvent = ref(false);
const sliderValue = ref(0);


const downloadSpeedLimit = computed(() => {
	return props.downloadSpeedLimit ?? 0;
});

const value = computed(() => {
	if (mouseEvent.value) {
		return sliderValue.value;
	}
	return downloadSpeedLimit.value;
});

function updateDownloadLimit(value: number): void {
	value = Number(value);
	if (value < 0) {
		value = 0;
	}
	emit('change', value);
}

</script>

