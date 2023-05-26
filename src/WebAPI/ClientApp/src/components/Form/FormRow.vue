<template>
	<q-row class="flex-nowrap" no-gutters>
		<q-col cols="6">
			<HelpIcon :label-id="labelId" :help-id="formId" />
		</q-col>
		<q-col cols="6" align-self="end">
			<slot />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
defineProps<{
	formId?: string;
	labelId?: string;
}>();
</script>
