<template>
	<v-data-table
		fixed-header
		show-select
		disable-pagination
		hide-default-footer
		:headers="getHeaders"
		:items="movies"
		:server-items-length="movies.length"
		:dark="$vuetify.theme.dark"
		:loading="loading"
	>
		<template v-slot:item.actions="{ item }">
			<v-icon small @click="downloadMovie(item)">
				mdi-download
			</v-icon>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import IPlexMovie from '@dto/IPlexMovie';
import { DataTableHeader } from 'vuetify/types';
import IPlexAccount from '@dto/IPlexAccount';
import DownloadService from '@service/downloadService';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadPlexMovie } from '@/types/api/plexDownloadApi';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class MovieTable extends Vue {
	@Prop({ required: true, type: Object as () => IPlexAccount })
	readonly activeAccount!: IPlexAccount;

	@Prop({ required: true, type: Array as () => IPlexMovie[] })
	readonly movies!: IPlexMovie[];

	@Prop({ required: true, type: Boolean, default: true })
	readonly loading!: Boolean;

	get getHeaders(): DataTableHeader<IPlexMovie>[] {
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
				text: 'Year',
				value: 'year',
			},
			{
				text: 'Added At',
				value: 'addedAt',
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
			},
			{
				text: 'Actions',
				value: 'actions',
				sortable: false,
			},
		];
	}

	downloadMovie(item: IPlexMovie): void {
		downloadPlexMovie(item.id, this.activeAccount?.id ?? 0).subscribe(() => {
			DownloadService.fetchDownloadList();
		});
	}
}
</script>
