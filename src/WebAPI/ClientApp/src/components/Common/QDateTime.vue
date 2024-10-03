<template>
	<span> {{ dateTimeString }}</span>
</template>

<script setup lang="ts">
import { format } from 'date-fns';
import { useSettingsStore } from '~/store';

const settingsStore = useSettingsStore();

const props = withDefaults(defineProps<{ text?: string; shortDate?: boolean; longDate?: boolean; time?: boolean }>(), {
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
		string += format(date.value, settingsStore.dateTimeSettings.timeFormat);
	}
	if (props.time && (props.shortDate || props.longDate)) {
		string += ' - ';
	}

	if (props.shortDate && settingsStore.dateTimeSettings.shortDateFormat) {
		string += format(date.value, settingsStore.dateTimeSettings.shortDateFormat);
	}

	if (props.longDate && settingsStore.dateTimeSettings.longDateFormat) {
		string += format(date.value, settingsStore.dateTimeSettings.longDateFormat);
	}

	return string;
});
</script>
