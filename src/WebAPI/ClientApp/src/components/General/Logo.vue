<template>
	<q-img
		no-spinner
		:style="{ width: props.size + 'px', height: props.size + 'px' }"
		src="/img/logo/full-logo-256.png"
		alt="Loading PlexRipper" />
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{ size: number }>(), {
	size: 128,
});
</script>
