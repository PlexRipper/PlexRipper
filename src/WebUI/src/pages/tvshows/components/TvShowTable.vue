<template>
	<v-row justify="center">
		<!-- Loading -->
		<v-col v-if="loading" cols="auto" align-self="center">
			<v-layout row justify-center align-center>
				<v-progress-circular :size="70" :width="7" color="red" indeterminate></v-progress-circular>
			</v-layout>
			<h1>Retrieving library from database</h1>
		</v-col>
		<v-col v-else cols="12">
			<!-- Table Headers -->
			<v-row class="table-header">
				<v-col class="ml-6" style="max-width: 50px">
					<v-checkbox
						:dark="$vuetify.theme.dark"
						:indeterminate="isIndeterminate"
						color="red"
						@change="selectAll($event)"
					></v-checkbox>
				</v-col>
				<v-col>
					Title
				</v-col>
				<v-col class="year-column">Year </v-col>
				<v-col class="type-column">Type </v-col>
				<v-col class="action-column">Actions </v-col>
			</v-row>
			<!-- Treeview Table -->
			<v-row no-gutters>
				<v-col>
					<v-treeview
						v-model="selected"
						selectable
						selected-color="red"
						selection-type="leaf"
						:dark="$vuetify.theme.dark"
						hoverable
						expand-icon="mdi-chevron-down"
						:items="getItems"
						transition
					>
						<template v-slot:label="{ item }">
							<v-row>
								<v-col class="ml-4">{{ item.name }} </v-col>
							</v-row>
						</template>
						<template v-slot:append="{ item }">
							<v-row>
								<v-col class="year-column"> {{ item.year }} </v-col>
								<v-col class="type-column"> {{ item.type }}</v-col>
								<v-col class="action-column" style="text-align: center;">
									<v-icon small @click="downloadMovie(item.id, item.type)">
										mdi-download
									</v-icon>
								</v-col>
							</v-row>
						</template>
					</v-treeview>
				</v-col>
			</v-row>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import DownloadService from '@service/downloadService';
import { PlexAccountDTO, PlexTvShowDTO, PlexMediaType } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadTvShow } from '@/types/api/plexDownloadApi';
import ITreeViewItem from '../types/iTreeViewItem';

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
	selected: number[] = [];

	get getItems(): ITreeViewItem[] {
		const items: ITreeViewItem[] = [];
		this.tvshows.forEach((x) => {
			const seasons: ITreeViewItem[] = [];
			if (x.seasons) {
				x.seasons.forEach((season) => {
					const episodes: ITreeViewItem[] = [];
					if (season.episodes) {
						season.episodes.forEach((episode) => {
							episodes.push({ id: episode.id, name: episode.title ?? '', type: 'episode', children: [] });
						});
						seasons.push({ id: season.id, name: season.title ?? '', type: 'season', children: episodes });
					}
				});
				items.push({ id: x.id, name: x.title ?? '', year: x.year, type: 'tvshow', children: seasons });
			}
		});
		return items;
	}

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

	get getLeafs(): number[] {
		return this.getItems.map((x) => x.children?.map((y) => y.children?.map((z) => z.id))).flat(2);
	}

	get isIndeterminate(): boolean {
		return this.getLeafs.length !== this.selected.length && this.selected.length > 0;
	}

	selectAll(state: boolean): void {
		Log.debug(state);
		if (state) {
			this.selected = this.getLeafs;
		} else {
			this.selected = [];
		}
	}

	downloadMovie(itemId: number, type: PlexMediaType): void {
		downloadTvShow(itemId, this.activeAccount?.id ?? 0, type).subscribe(() => {
			DownloadService.fetchDownloadList();
		});
	}
}
</script>

<style lang="scss">
.v-treeview-node {
	background: #505050 !important;
	border-top: 0.888889px solid rgba(255, 255, 255, 0.377);
}

.table-header {
	background: rgb(30, 30, 30);
	border-top: 0.888889px solid rgba(255, 255, 255, 0.377);
	margin: 0;
	height: 50px;
	.col {
		.v-input--selection-controls {
			padding: 0 0 0 2px !important;
			margin: 0 !important;
		}
	}
}
.year-column {
	min-width: 80px;
	max-width: 80px;
}
.type-column {
	min-width: 100px;
	max-width: 100px;
}
.action-column {
	min-width: 100px;
	max-width: 100px;
}
</style>
