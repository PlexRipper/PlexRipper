<template>
	<BaseButton :icon="icon" icon-only @click="toggleDarkMode" />
</template>

<script setup lang="ts">
const $q = useQuasar();

const icon = computed((): string => {
	return $q.dark.isActive ? 'mdi-white-balance-sunny' : 'mdi-moon-waxing-crescent';
});

function toggleDarkMode(): void {
	$q.dark.toggle();
}
</script>
