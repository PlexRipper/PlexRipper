<template>
	<IconButton
		:icon="icon"
		@click="toggleAnimationMode" />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();

const icon = computed((): string => {
	return settingsStore.generalSettings.disableAnimatedBackground ? 'mdi-play' : 'mdi-stop';
});

function toggleAnimationMode(): void {
	settingsStore.generalSettings.disableAnimatedBackground = !settingsStore.generalSettings.disableAnimatedBackground;
}
</script>
