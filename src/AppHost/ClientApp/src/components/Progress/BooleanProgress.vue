<template>
	<q-circular-progress
		v-if="loading"
		indeterminate
		rounded
		size="32px"
		color="red" />
	<q-icon
		v-else-if="success"
		name="mdi-check" />
	<q-icon
		v-else
		name="mdi-close" />
</template>

<script setup lang="ts">
defineProps<{
	loading?: boolean | null;
	success?: boolean | null;
}>();
</script>
