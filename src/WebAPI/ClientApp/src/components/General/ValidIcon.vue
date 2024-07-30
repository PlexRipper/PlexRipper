<template>
	<q-icon
		class="valid-icon"
		size="30px"
		:color="valid ? 'green' : 'red'"
		:name="getIcon"
		style="margin: 10px">
		<q-tooltip
			anchor="top middle"
			self="bottom middle"
			:offset="[10, 10]"
			:disabled="valid">
			<span>{{ valid ? validText : invalidText }}</span>
		</q-tooltip>
	</q-icon>
</template>

<script setup lang="ts">
const props = withDefaults(
	defineProps<{
		valid: boolean;
		validText: string;
		invalidText: string;
	}>(),
	{},
);

const getIcon = computed((): string => (props.valid ? 'mdi-check-circle-outline' : 'mdi-alert-circle'));
</script>
