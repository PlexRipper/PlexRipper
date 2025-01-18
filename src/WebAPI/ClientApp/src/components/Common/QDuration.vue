<template>
	<QText
		:align="align"
		:value="duration"
		:cy="cy" />
</template>

<script setup lang="ts">
import { formatDuration, intervalToDuration } from 'date-fns';
import type { IQTextProps } from '@interfaces';

const props = defineProps<Pick<IQTextProps, 'align'> & {
	value: number;
	short?: boolean;
	cy?: string;
}>();

const duration = computed(() => {
	if (!props.value) {
		return '-';
	}

	if (props.short) {
		const duration = intervalToDuration({ start: 0, end: props.value * 1000 });
		const zeroPad = (num: number | undefined) => String(num || 0).padStart(2, '0');
		const minutes = zeroPad(duration.minutes);
		const seconds = zeroPad(duration.seconds);

		if (duration.hours) {
			return `${zeroPad(duration.hours)}:${minutes}:${seconds}`;
		}

		return `${minutes}:${seconds}`;
	}

	const date = new Date(props.value * 1000);
	return formatDuration(
		{ hours: date.getUTCHours(), minutes: date.getUTCMinutes(), seconds: date.getUTCSeconds() },
		{ delimiter: ' ', format: ['hours', 'minutes', 'seconds'] },
	);
});
</script>
