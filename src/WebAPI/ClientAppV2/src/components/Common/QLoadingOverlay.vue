<template>
	<q-inner-loading :showing="loading">
		<q-spinner-gears :size="`${size}px`" color="red" />
	</q-inner-loading>
</template>

<script setup lang="ts">
import { defineProps, withDefaults } from 'vue';

withDefaults(
	defineProps<{
		loading?: boolean;
		size?: number;
	}>(),
	{
		loading: false,
		size: 100,
	},
);
</script>
