<template>
	<QPage>
		<MediaOverview
			:library-id="0"
			all-media-mode
			:media-type="settingsStore.displaySettings.allOverviewViewMode" />
	</QPage>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();
</script>
