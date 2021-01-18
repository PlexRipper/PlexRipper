<template>
	<v-data-table
		fixed-header
		show-select
		hide-default-footer
		:headers="getHeaders"
		:items-per-page="30"
		:items="downloads"
		:loading="loading"
		:value="value"
		@input="$emit('input', $event)"
	>
		<!-- Data received -->
		<template #item.dataReceived="{ item }">
			<strong>
				<file-size :size="item.dataReceived" />
			</strong>
		</template>
		<!-- Data total -->
		<template #item.dataTotal="{ item }">
			<strong>
				<file-size :size="item.dataTotal" />
			</strong>
		</template>
		<!-- Download speed -->
		<template #item.downloadSpeed="{ item }">
			<strong v-if="item.downloadSpeed > 0">
				<file-size :size="item.downloadSpeed" speed />
			</strong>
			<strong v-else> - </strong>
		</template>
		<!-- Download Time Remaining -->
		<template #item.timeRemaining="{ item }">
			<strong> {{ formatCountdown(item.timeRemaining) }} </strong>
		</template>
		<!-- Percentage -->
		<template #item.percentage="{ item }">
			<v-progress-linear :value="item.percentage" stream striped color="red" height="25">
				<template #default="{ value }">
					<strong>{{ value }}%</strong>
				</template>
			</v-progress-linear>
		</template>
		<!-- Actions -->
		<template #item.actions="{ item }">
			<v-btn-toggle borderless dense group tile>
				<template v-for="(action, i) in availableActions(item)">
					<!-- Render buttons -->
					<template v-for="(button, y) in getButtons">
						<v-btn v-if="action === button.value" :key="`${i}-${y}`" icon @click="command(action, item.id)">
							<v-tooltip top>
								<template #activator="{ on, attrs }">
									<!-- Button icon-->
									<v-icon v-bind="attrs" v-on="on"> {{ button.icon }} </v-icon>
								</template>
								<!-- Tooltip text -->
								<span>{{ button.name }}</span>
							</v-tooltip>
						</v-btn>
					</template>
				</template>
			</v-btn-toggle>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DownloadStatus } from '@dto/mainApi';
import { DataTableHeader } from 'vuetify/types';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import IDownloadRow from '../types/IDownloadRow';
@Component({
	components: {
		LoadingSpinner,
	},
})
export default class DownloadsTable extends Vue {
	@Prop({ required: true, type: Array as () => IDownloadRow[] })
	readonly downloads!: IDownloadRow[];

	@Prop({ type: Boolean })
	readonly loading: Boolean = false;

	@Prop({ required: true, type: Array as () => IDownloadRow[] })
	readonly value!: IDownloadRow[];

	get getHeaders(): DataTableHeader<IDownloadRow>[] {
		return [
			// {
			// 	text: 'Id',
			// 	value: 'id',
			// },
			{
				text: 'Title',
				value: 'title',
			},
			{
				text: 'Status',
				value: 'status',
			},
			{
				text: 'Data Received',
				value: 'dataReceived',
			},
			{
				text: 'Size',
				value: 'dataTotal',
			},
			{
				text: 'Speed',
				value: 'downloadSpeed',
			},
			{
				text: 'ETA',
				value: 'timeRemaining',
			},
			{
				text: 'Percentage',
				value: 'percentage',
			},
			{
				text: 'Actions',
				value: 'actions',
				width: '100px',
				sortable: false,
			},
		];
	}

	get getButtons(): any {
		return [
			{
				name: 'Restart',
				value: 'restart',
				icon: 'mdi-refresh',
			},
			{
				name: 'Start / Resume',
				value: 'start',
				icon: 'mdi-play',
			},
			{
				name: 'Pause',
				value: 'pause',
				icon: 'mdi-pause',
			},
			{
				name: 'Stop',
				value: 'stop',
				icon: 'mdi-stop',
			},
			{
				name: 'Delete',
				value: 'delete',
				icon: 'mdi-delete',
			},
			{
				name: 'Clear',
				value: 'clear',
				icon: 'mdi-notification-clear-all',
			},
			{
				name: 'Details',
				value: 'details',
				icon: 'mdi-chart-box-outline',
			},
		];
	}

	formatCountdown(seconds: number): string {
		if (!seconds || seconds <= 0) {
			return '0:00';
		}
		return new Date(seconds * 1000)?.toISOString()?.substr(11, 8)?.toString() ?? '?';
	}

	availableActions(item: IDownloadRow): string[] {
		const actions: string[] = ['details'];
		switch (item.status) {
			case DownloadStatus.Initialized:
				actions.push('delete');
				break;
			case DownloadStatus.Starting:
				actions.push('stop');
				actions.push('delete');
				break;
			case DownloadStatus.Queued:
				actions.push('start');
				actions.push('delete');
				break;
			case DownloadStatus.Downloading:
				actions.push('pause');
				actions.push('stop');
				break;
			case DownloadStatus.Paused:
				actions.push('start');
				actions.push('restart');
				actions.push('stop');
				break;
			case DownloadStatus.Completed:
				actions.push('clear');
				break;
			case DownloadStatus.Stopping:
				actions.push('delete');
				break;
			case DownloadStatus.Stopped:
				actions.push('restart');
				actions.push('delete');
				break;
			case DownloadStatus.Merging:
				break;
			case DownloadStatus.Error:
				actions.push('restart');
				actions.push('delete');
				break;
		}
		return actions;
	}

	command(action: string, itemId: number): void {
		this.$emit(action, itemId);
	}
}
</script>
