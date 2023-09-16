<template>
	<span> {{ duration }}</span>
</template>

<script setup lang="ts">
import { formatDuration, intervalToDuration } from 'date-fns';

const props = defineProps<{
	value: number;
	short?: boolean;
}>();

const duration = computed(() => {
	if (!props.value) {
		return '-';
	}

	if (props.short) {
		const duration = intervalToDuration({ start: 0, end: props.value * 1000 });
		const zeroPad = (num) => String(num).padStart(2, '0');
		return [duration.hours, duration.minutes, duration.seconds].filter(Boolean).map(zeroPad).join(':');
	}

	const date = new Date(props.value * 1000);
	return formatDuration(
		{ hours: date.getUTCHours(), minutes: date.getUTCMinutes(), seconds: date.getSeconds() },
		{ delimiter: ' ', format: ['hours', 'minutes', 'seconds'] },
	);
});
</script>
