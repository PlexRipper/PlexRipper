<template>
	<!-- Download Toolbar -->
	<q-row no-gutters>
		<q-col>
			<q-toolbar outlined :height="64">
				<!--Command buttons-->
				<q-btn
					v-for="(button, i) in buttons"
					:key="i"
					:icon="button.icon"
					depressed
					tile
					:disable="button.disableOnNoSelected && !hasSelected"
					class="no-background"
					@click="$emit(button.value)">
					<span class="hidden-sm-and-down">{{ button.name }}</span>
				</q-btn>
			</q-toolbar>
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';

interface buttons {
	name: string;
	value: string;
	icon: string;
	disableOnNoSelected: boolean;
}

defineProps<{
	hasSelected: boolean;
}>();

const buttons = computed<buttons[]>(() => {
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
