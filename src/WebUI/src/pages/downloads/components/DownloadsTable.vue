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
		<!-- Download speed -->
		<template v-slot:item.timeRemaining="{ item }">
			<strong> {{ [item.timeRemaining, 'minutes'] | duration('humanize', true) }} </strong>
		</template>
		<!-- Percentage -->
		<template v-slot:item.percentage="{ item }">
			<v-progress-linear v-model="item.percentage" striped color="blue-grey" :dark="$vuetify.theme.dark" height="25">
				<template v-slot="{ value }">
					<strong>{{ value }}%</strong>
				</template>
			</v-progress-linear>
		</template>
		<!-- Actions -->
		<template v-slot:item.actions="{ item }">
			<v-btn icon @click="pauseMovie(item)">
				<v-icon> mdi-pause </v-icon>
			</v-btn>
			<v-btn icon @click="pauseMovie(item)">
				<v-icon> mdi-stop </v-icon>
			</v-btn>
			<v-btn icon @click="deleteMovie(item)">
				<v-icon> mdi-delete </v-icon>
			</v-btn>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import { PlexMovieDTO } from '@dto/mainApi';
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

	get getHeaders(): DataTableHeader<IDownloadRow>[] {
		return [
			{
				text: 'Id',
				value: 'id',
			},
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
				sortable: false,
			},
		];
	}

	pauseMovie(item: PlexMovieDTO): void {
		Log.debug(item);
	}

	cancelMovie(item: PlexMovieDTO): void {
		Log.debug(item);
	}

	deleteMovie(item: IDownloadRow): void {
		this.$emit('delete', item.id);
	}
}
</script>
