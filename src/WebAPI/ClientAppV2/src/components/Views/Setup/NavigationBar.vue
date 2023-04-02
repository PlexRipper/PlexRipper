<template>
	<q-row justify="center">
		<q-col cols="auto" style="position: absolute; left: 0">
			<dark-mode-toggle />
		</q-col>
		<q-space />
		<q-col v-if="!isLast" cols="2">
			<NavigationPreviousButton
				class="mx-2"
				:disabled="disableBack"
				cy="setup-page-previous-button"
				@click="$emit('back')" />
		</q-col>
		<q-col v-if="!isLast" cols="2">
			<NavigationNextButton class="mx-2" :disabled="disableNext" cy="setup-page-next-button" @click="$emit('next')" />
		</q-col>
		<q-col v-else cols="3">
			<NavigationFinishSetupButton class="mx-2" cy="setup-page-skip-setup-button" @click="$emit('finish')" />
		</q-col>
		<q-space />
	</q-row>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from 'vue';

defineProps<{
	disableBack: boolean;
	disableNext: boolean;
	isLast: boolean;
}>();

defineEmits<{
	(e: 'back'): void;
	(e: 'next'): void;
	(e: 'finish'): void;
}>();
</script>
