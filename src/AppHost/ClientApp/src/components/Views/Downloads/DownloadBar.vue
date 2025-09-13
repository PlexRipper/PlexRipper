<template>
	<q-toolbar class="download-overview-bar">
		<!-- Download Toolbar -->
		<QRow
			no-gutters
			justify="end">
			<!-- Command buttons -->
			<QCol
				v-for="(button, i) in buttons"
				:key="i"
				cols="auto">
				<VerticalButton
					:icon="button.icon"
					:label="button.name"
					:disabled="button.disableOnNoSelected && !downloadStore.hasSelected"
					:width="verticalButtonWidth"
					@click="onDownloadAction(button.value)" />
			</QCol>
		</QRow>
	</q-toolbar>
</template>

<script setup lang="ts">
import { useDownloadStore } from '@store';
import { DownloadActions } from '@dto';

const downloadStore = useDownloadStore();

const verticalButtonWidth = ref(120);

const buttons = computed<
	{
		name: string;
		value: DownloadActions;
		icon: string;
		disableOnNoSelected: boolean;
	}[]
>(() => {
	return [
		{
			name: 'Clear Completed',
			value: DownloadActions.Clear,
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
			value: DownloadActions.Delete,
			icon: 'mdi-delete',
			disableOnNoSelected: true,
		},
	];
});

function onDownloadAction(action: DownloadActions) {
	useSubscription(downloadStore.executeBatchDownloadCommand(action).subscribe());
}
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;
@use '@/assets/scss/mixins';

.download-overview-bar {
  @extend .default-border;
  max-height: $download-page-bar-height;
}
</style>
