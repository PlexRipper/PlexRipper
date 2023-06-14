<template>
	<q-row>
		<q-col cols="4" align-self="center">
			<help-icon :help-id="helpId" />
		</q-col>
		<q-col cols="4" align-self="center">
			<slot />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
defineProps<{
	helpId: string;
}>();
</script>
