<template>
	<q-toolbar class="download-overview-bar">
		<!-- Download Toolbar -->
		<q-row no-gutters justify="end">
			<!--Command buttons-->
			<q-col v-for="(button, i) in buttons" :key="i" cols="auto">
				<vertical-button
					:icon="button.icon"
					:label="button.name"
					:disabled="button.disableOnNoSelected && !hasSelected"
					:width="verticalButtonWidth"
					@click="$emit('action', button.value)" />
			</q-col>
		</q-row>
	</q-toolbar>
</template>

<script setup lang="ts">
interface IButton {
	name: string;
	value: string;
	icon: string;
	disableOnNoSelected: boolean;
}

defineProps<{
	hasSelected: boolean;
}>();

defineEmits<{ (e: 'action', action: string): void }>();

const verticalButtonWidth = ref(120);

const buttons = computed<IButton[]>(() => {
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
