<template>
	<span v-if="size > 0">{{ formattedString }}{{ speed ? $t('general.units.per-second') : '' }}</span>
	<span v-else> - </span>
</template>

<script setup lang="ts">
const props = defineProps<{
	size: number;
	speed?: boolean;
}>();

const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB', 'BB'];

/*
 * Filesize filters
 * Source: https://github.com/sainf/vue-filter-pretty-bytes
 */
const formattedString = computed(() => {
	const bytes = props.size;
	const kib = false;
	const decimals = 2;

	if (bytes === 0) return '0 Bytes';
	if (isNaN(bytes) && !isFinite(bytes)) return 'Unknown';
	const k = kib ? 1024 : 1000;
	const dm = !isNaN(decimals) && decimals >= 0 ? decimals : 2;

	// const sizes = kib? ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB', 'BiB']

	const i = Math.floor(Math.log(bytes) / Math.log(k));

	return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
});
</script>
