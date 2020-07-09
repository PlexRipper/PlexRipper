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
		<template v-slot:item.progress="{ item }">
			<v-progress-linear v-model="item.progress" color="blue-grey" :dark="$vuetify.theme.dark" height="25">
				<template v-slot="{ value }">
					<strong>{{ Math.ceil(value) }}%</strong>
				</template>
			</v-progress-linear>
		</template>
		<template v-slot:item.actions="{ item }">
			<v-icon small @click="downloadMovie(item)">
				mdi-download
			</v-icon>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { IPlexMovie } from '@dto/IPlexMovie';
import { DataTableHeader } from 'vuetify/types';
import IDownloadTask from '@dto/IDownloadTask';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadPlexMovie } from '@/types/api/plexDownloadApi';
import { UserStore } from '@/store/';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class DownloadsTable extends Vue {
	@Prop({ required: true, type: Array as () => IDownloadTask[] })
	readonly downloads!: IDownloadTask[];

	@Prop({ type: Boolean })
	readonly loading: Boolean = false;

	get getHeaders(): DataTableHeader<IDownloadTask>[] {
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
				text: 'Progress',
				value: 'progress',
			},
			{
				text: 'Actions',
				value: 'actions',
				sortable: false,
			},
		];
	}

	downloadMovie(item: IPlexMovie): void {
		Log.debug(item);
		downloadPlexMovie(item.id, UserStore.getAccountId);
	}
}
</script>
