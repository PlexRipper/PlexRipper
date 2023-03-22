<template>
	<q-input v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';

const props = withDefaults(
	defineProps<{
		height: number;
		dense: boolean;
		outlined: boolean;
		solo: boolean;
	}>(),
	{
		height: 50,
		dense: true,
		outlined: true,
		solo: true,
	},
);
</script>
