<template>
	<v-data-table
		fixed-header
		show-select
		disable-pagination
		hide-default-footer
		:headers="getHeaders"
		:items="tvshows"
		:server-items-length="tvshows.length"
		:dark="$vuetify.theme.dark"
		:loading="loading"
		show-expand
		class="season-table"
		:expanded.sync="expanded"
	>
		<template v-slot:top>
			<v-toolbar flat>
				<v-toolbar-title>Expandable Table</v-toolbar-title>
				<v-spacer></v-spacer>
				<v-switch v-model="singleExpand" label="Single expand" class="mt-2"></v-switch>
			</v-toolbar>
		</template>
		<template v-slot:expanded-item="{ headers, item }">
			<td :colspan="24">
				<v-data-table
					:headers="headers"
					:items="item.seasons"
					show-select
					disable-pagination
					hide-default-header
					hide-default-footer
					show-expand
					:server-items-length="item.seasons.length"
					:dark="$vuetify.theme.dark"
				>
					<template v-slot:expanded-item="{ item }">
						<td :colspan="24">
							<v-data-table
								style="margin-left: 125px;"
								:headers="getHeaders"
								:items="item.episodes"
								show-select
								disable-pagination
								hide-default-header
								hide-default-footer
								:dark="$vuetify.theme.dark"
							>
							</v-data-table>
						</td>
					</template>
				</v-data-table>
			</td>
		</template>
		<template v-slot:item.actions="{ item }">
			<v-icon small @click="downloadMovie(item)">
				mdi-download
			</v-icon>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import DownloadService from '@service/downloadService';
import { PlexAccountDTO, PlexTvShowDTO } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadPlexMovie } from '@/types/api/plexDownloadApi';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class TVShowsTable extends Vue {
	@Prop({ required: true, type: Object as () => PlexAccountDTO })
	readonly activeAccount!: PlexAccountDTO;

	@Prop({ required: true, type: Array as () => PlexTvShowDTO[] })
	readonly tvshows!: PlexTvShowDTO[];

	@Prop({ required: true, type: Boolean, default: true })
	readonly loading!: Boolean;

	expanded: string[] = [];
	singleExpand: boolean = false;

	get getHeaders(): DataTableHeader<PlexTvShowDTO>[] {
		return [
			// {
			// 	text: 'Id',
			// 	value: 'id',
			// },
			{
				text: 'Title',
				value: 'title',
				width: 500,
			},
			{
				text: 'Year',
				value: 'year',
				width: 50,
			},
			{
				text: 'Added At',
				value: 'addedAt',
				width: 150,
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
				width: 150,
			},
			{
				text: 'Actions',
				value: 'actions',
				width: 50,
				sortable: false,
			},
		];
	}

	downloadMovie(item: PlexTvShowDTO): void {
		downloadPlexMovie(item?.id ?? 0, this.activeAccount?.id ?? 0).subscribe(() => {
			DownloadService.fetchDownloadList();
		});
	}
}
</script>

<style lang="scss" scoped>
.text-start {
	background: floralwhite !important;
	max-width: 40px !important;
}
</style>
