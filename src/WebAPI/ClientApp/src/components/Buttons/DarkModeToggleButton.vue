<template>
	<IconButton
		:icon="icon"
		@click="toggleDarkMode"
	/>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const $q = useQuasar();

const icon = computed((): string => {
	return $q.dark.isActive ? 'mdi-white-balance-sunny' : 'mdi-moon-waxing-crescent';
});

function toggleDarkMode(): void {
	$q.dark.toggle();
}
</script>
