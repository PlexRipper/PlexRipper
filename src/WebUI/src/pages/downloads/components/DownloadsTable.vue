<template>
	<v-data-table
		fixed-header
		show-select
		disable-pagination
		hide-default-footer
		:headers="getHeaders"
		:items="downloads"
		:server-items-length="downloads.length"
		:dark="$vuetify.theme.dark"
		:loading="loading"
		:value="value"
		@input="$emit('input', $event)"
	>
		<!-- Data received -->
		<template v-slot:item.dataReceived="{ item }">
			<strong>
				{{ item.dataReceived | prettyBytes }}
			</strong>
		</template>
		<!-- Data total -->
		<template v-slot:item.dataTotal="{ item }">
			<strong>
				{{ item.dataTotal | prettyBytes }}
			</strong>
		</template>
		<!-- Download speed -->
		<template v-slot:item.downloadSpeed="{ item }">
			<strong> {{ item.downloadSpeed | prettyBytes }}/s </strong>
		</template>
		<!-- Download Time Remaining -->
		<template v-slot:item.timeRemaining="{ item }">
			<strong> {{ formatCountdown(item.timeRemaining) }} </strong>
		</template>
		<!-- Percentage -->
		<template v-slot:item.percentage="{ item }">
			<v-progress-linear :value="item.percentage" stream striped color="blue-grey" :dark="$vuetify.theme.dark" height="25">
				<template v-slot="{ value }">
					<strong>{{ value }}%</strong>
				</template>
			</v-progress-linear>
		</template>
		<!-- Actions -->
		<template v-slot:item.actions="{ item }">
			<v-btn-toggle borderless dense group tile :dark="$vuetify.theme.dark">
				<template v-for="(action, i) in availableActions(item)">
					<!-- Render buttons -->
					<template v-for="(button, y) in getButtons">
						<v-btn v-if="action === button.value" :key="`${i}-${y}`" icon @click="command(action, item.id)">
							<v-tooltip top>
								<template v-slot:activator="{ on, attrs }">
									<!-- Button icon-->
									<v-icon v-bind="attrs" :dark="$vuetify.theme.dark" v-on="on"> {{ button.icon }} </v-icon>
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

	@Prop({ type: Array as () => IDownloadRow[] })
	readonly value: IDownloadRow[] = [];

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
				text: 'Data Total',
				value: 'dataTotal',
			},
			{
				text: 'Download Speed',
				value: 'downloadSpeed',
			},
			{
				text: 'Time Remaining',
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
				name: 'Details',
				value: 'details',
				icon: 'mdi-chart-box-outline',
			},
		];
	}

	formatCountdown(seconds: number): string {
		if (seconds <= 0) {
			return '00:00';
		}

		return new Date(seconds * 1000).toISOString().substr(11, 8).toString();
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
			case DownloadStatus.Downloading:
				actions.push('pause');
				actions.push('stop');
				break;
			case DownloadStatus.Completed:
				actions.push('restart');
				actions.push('delete');
				break;
			case DownloadStatus.Stopped:
				actions.push('restart');
				actions.push('delete');
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
