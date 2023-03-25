<template>
	<span> {{ duration }}</span>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';
import { formatDuration } from 'date-fns';

const props = defineProps<{
	value: number;
}>();

const duration = computed(() => {
	const date = new Date(props.value * 1000);
	return formatDuration(
		{ hours: date.getUTCHours(), minutes: date.getUTCMinutes(), seconds: date.getSeconds() },
		{ delimiter: ' ', format: ['hours', 'minutes', 'seconds'] },
	);
});
</script>
