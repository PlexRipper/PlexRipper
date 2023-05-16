<template>
	<span> {{ dateTimeString }}</span>
</template>

<script setup lang="ts">
import { format } from 'date-fns';
import { useSubscription } from '@vueuse/rxjs';
import { SettingsService } from '@service';

const shortDateFormat = ref('dd/MM/yyyy');
const longDateFormat = ref('EEEE, dd MMMM yyyy');
const timeFormat = ref('HH:mm:ss');

const props = withDefaults(defineProps<{ text: string; shortDate?: boolean; longDate?: boolean; time?: boolean }>(), {
	text: '',
	shortDate: false,
	longDate: false,
	time: false,
});

const date = computed(() => {
	return new Date(props.text);
});

const dateTimeString = computed(() => {
	if (!props.text) {
		return '';
	}
	let string = '';
	if (props.time) {
		string += format(date.value, timeFormat.value);
	}
	if (props.time && (props.shortDate || props.longDate)) {
		string += ' - ';
	}

	if (props.shortDate && shortDateFormat.value) {
		string += format(date.value, shortDateFormat.value);
	}

	if (props.longDate && longDateFormat.value) {
		string += format(date.value, longDateFormat.value);
	}

	return string;
});

onMounted(() => {
	useSubscription(
		SettingsService.getShortDateFormat().subscribe((value) => {
			shortDateFormat.value = value;
		}),
	);
	useSubscription(
		SettingsService.getLongDateFormat().subscribe((value) => {
			longDateFormat.value = value;
		}),
	);
	useSubscription(
		SettingsService.getTimeFormat().subscribe((value) => {
			timeFormat.value = value;
		}),
	);
});
</script>
