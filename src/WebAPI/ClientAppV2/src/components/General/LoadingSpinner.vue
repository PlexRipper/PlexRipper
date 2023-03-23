<template>
	<q-circular-progress size="xl" color="red" indeterminate />
</template>

<script setup lang="ts">
import { defineProps, withDefaults } from 'vue';

withDefaults(
	defineProps<{
		size?: number;
	}>(),
	{
		size: 32,
	},
);
</script>
