<template>
	<IconButton
		:icon="icon"
		@click="toggle" />
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();

const icon = computed((): string => {
	return settingsStore.generalSettings.disableAnimatedBackground
		? 'mdi-image-off-outline'
		: 'mdi-image-outline';
});

function toggle(): void {
	settingsStore.$patch({
		generalSettings: {
			disableAnimatedBackground: !settingsStore.generalSettings.disableAnimatedBackground,
		},
	});
}
</script>
