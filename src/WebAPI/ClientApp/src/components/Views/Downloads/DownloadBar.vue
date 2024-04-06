<template>
	<q-toolbar class="download-overview-bar">
		<!-- Download Toolbar -->
		<q-row no-gutters justify="end">
			<!--Command buttons-->
			<q-col v-for="(button, i) in buttons" :key="i" cols="auto">
				<vertical-button
					:icon="button.icon"
					:label="button.name"
					:disabled="button.disableOnNoSelected && !downloadStore.hasSelected"
					:width="verticalButtonWidth"
					@click="downloadStore.executeBatchDownloadCommand(button.value)" />
			</q-col>
		</q-row>
	</q-toolbar>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useDownloadStore } from '@store';

const downloadStore = useDownloadStore();

const verticalButtonWidth = ref(120);

const buttons = computed<
	{
		name: string;
		value: string;
		icon: string;
		disableOnNoSelected: boolean;
	}[]
>(() => {
	return [
		{
			name: 'Clear Completed',
			value: 'clear',
			icon: 'mdi-notification-clear-all',
			disableOnNoSelected: true,
		},
		// {
		// 	name: 'Start',
		// 	value: 'start',
		// 	icon: 'mdi-play',
		// 	disableOnNoSelected: true,
		// },
		// {
		// 	name: 'Pause',
		// 	value: 'pause',
		// 	icon: 'mdi-pause',
		// 	disableOnNoSelected: true,
		// },
		// {
		// 	name: 'Stop',
		// 	value: 'stop',
		// 	icon: 'mdi-stop',
		// 	disableOnNoSelected: true,
		// },
		// {
		// 	name: 'Restart',
		// 	value: 'restart',
		// 	icon: 'mdi-restart',
		// 	disableOnNoSelected: true,
		// },
		{
			name: 'Delete',
			value: 'delete',
			icon: 'mdi-delete',
			disableOnNoSelected: true,
		},
	];
});
</script>
