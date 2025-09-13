<template>
	<QText
		:value="countdown"
		align="center" />
</template>

<script setup lang="ts">
const props = defineProps<{
	value: string;
}>();

const { t } = useI18n();

const countdown = computed(() => {
	if (!props.value) {
		return '-';
	}

	// Split the timeSpan into hours, minutes, seconds, and milliseconds
	const [hours, minutes, secondsWithMilliseconds] = props.value.split(':');

	// Further split seconds from milliseconds
	const [seconds] = secondsWithMilliseconds.split('.');

	// Convert to integers for further calculations if needed
	const hoursInt = parseInt(hours, 10);
	const minutesInt = parseInt(minutes, 10);
	const secondsInt = parseInt(seconds, 10);

	const result = '';

	if (hoursInt > 0) {
		return t('components.q-countdown.hours-minute-seconds', { hours: hoursInt, minutes: minutesInt, seconds: secondsInt });
	}

	if (minutesInt > 0) {
		return t('components.q-countdown.minutes-seconds', { minutes: minutesInt, seconds: secondsInt });
	}

	if (secondsInt > 0) {
		return t('components.q-countdown.seconds', { seconds: secondsInt });
	}

	return result;
});
</script>
