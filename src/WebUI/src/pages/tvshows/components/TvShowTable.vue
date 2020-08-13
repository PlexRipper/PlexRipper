<template>
	<v-row justify="center">
		<v-col cols="12">
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
						item-key="key"
					>
						<template v-slot:label="{ item }">
							<v-row>
								<v-col class="ml-4">{{ item.name }} </v-col>
							</v-row>
						</template>
						<template v-slot:append="{ item }">
							<v-row>
								<v-col> {{ item.key }} </v-col>
								<v-col class="year-column"> {{ item.year }} </v-col>
								<v-col class="type-column"> {{ item.type }}</v-col>
								<v-col class="action-column" style="text-align: center;">
									<v-icon small @click="openDownloadConfirmationDialog(item.id, item.type)">
										mdi-download
									</v-icon>
								</v-col>
							</v-row>
						</template>
					</v-treeview>
				</v-col>
			</v-row>
		</v-col>
		<!-- The "Are you sure" dialog -->
		<v-col cols="12">
			<v-dialog v-model="showDialog" :max-width="500" :dark="$vuetify.theme.dark" scrollable>
				<v-card v-if="progress === null" :dark="$vuetify.theme.dark">
					<v-card-title>
						Are you sure?
					</v-card-title>
					<v-card-subtitle>
						<p>Plex Ripper will start downloading all of the following:</p>
					</v-card-subtitle>
					<!-- Show Download Task Preview -->
					<v-card-text>
						<v-treeview open-all :items="downloadPreview" :open.sync="openDownloadPreviews"></v-treeview>
					</v-card-text>
					<v-divider></v-divider>

					<v-card-actions>
						<v-btn large @click="showDialog = false">
							Cancel
						</v-btn>
						<v-spacer></v-spacer>
						<v-btn color="success" large @click="downloadTvShows()">
							Yes!
						</v-btn>
					</v-card-actions>
				</v-card>

				<!-- Download Task Creation Progressbar -->
				<v-card v-else>
					<v-card-title class="justify-center">
						Creating download tasks {{ progress.current }} of {{ progress.total }}
					</v-card-title>
					<v-card-text>
						<progress-component text="" :percentage="progress.percentage" />
					</v-card-text>
				</v-card>
			</v-dialog>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import DownloadService from '@service/downloadService';
import { PlexAccountDTO, PlexTvShowDTO, PlexMediaType, DownloadTaskCreationProgress } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadTvShow } from '@/types/api/plexDownloadApi';
import { clone } from 'lodash';
import { takeWhile, tap, finalize } from 'rxjs/operators';
import SignalrService from '@service/signalrService';
import { merge } from 'rxjs';
import ProgressComponent from '@/components/ProgressComponent.vue';
import ITreeViewItem from '../types/iTreeViewItem';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
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

	showDialog: boolean = false;
	downloadPreview: ITreeViewItem[] = [];
	downloadPreviewtype: PlexMediaType = 'None';
	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	get getItems(): ITreeViewItem[] {
		const items: ITreeViewItem[] = [];
		this.tvshows.forEach((tvShow) => {
			const seasons: ITreeViewItem[] = [];
			if (tvShow.seasons) {
				tvShow.seasons.forEach((season) => {
					const episodes: ITreeViewItem[] = [];
					if (season.episodes) {
						season.episodes.forEach((episode) => {
							// Add Episode
							episodes.push({
								id: episode.id,
								key: `${tvShow.id}-${season.id}-${episode.id}`,
								name: episode.title ?? '',
								type: 'Episode',
								children: [],
								item: episode,
							});
						});
						// Add seasons
						seasons.push({
							id: season.id,
							key: `${tvShow.id}-${season.id}`,
							name: season.title ?? '',
							type: 'Season',
							children: episodes,
							item: season,
						});
					}
				});
				// Add tvShow
				items.push({
					id: tvShow.id,
					key: `${tvShow.id}`,
					name: tvShow.title ?? '',
					year: tvShow.year,
					type: 'TvShow',
					item: tvShow,
					children: seasons,
				});
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

	createPreview(itemId: number, type: PlexMediaType): ITreeViewItem[] {
		const result: ITreeViewItem[] = [];
		this.openDownloadPreviews = [];

		// Tv show: Show tvshow -> with all seasons -> with all episodes
		if (type === 'TvShow') {
			const tvShow: ITreeViewItem | undefined = this.getItems.find((x) => x.id === itemId);
			if (tvShow) {
				// Ensure all nodes are open
				this.openDownloadPreviews.push(tvShow.id);
				tvShow.children.forEach((season) => {
					this.openDownloadPreviews.push(season.id);
				});
				result.push(clone(tvShow));
			}
		}

		// Season: Show tvshow -> season -> with all episodes
		if (type === 'Season') {
			let tvShow: ITreeViewItem = {} as ITreeViewItem;
			for (let i = 0; i < this.getItems.length; i++) {
				const season = this.getItems[i].children.find((c) => c.id === itemId);
				if (season) {
					tvShow = clone(this.getItems[i]);
					tvShow.children = clone([season]);
					// Ensure all nodes are open
					this.openDownloadPreviews.push(tvShow.id);
					this.openDownloadPreviews.push(season.id);
					break;
				}
			}
			Log.debug(tvShow);
			result.push(tvShow);
		}

		// Episode: Show tvshow -> season -> episode without anything else
		if (type === 'Episode') {
			let tvShow: ITreeViewItem = {} as ITreeViewItem;
			for (let i = 0; i < this.getItems.length; i++) {
				for (let j = 0; j < this.getItems[i].children.length; j++) {
					const season: ITreeViewItem = clone(this.getItems[i].children[j]);
					const episode: ITreeViewItem | undefined = season?.children?.find((c) => c.id === itemId);
					if (episode) {
						tvShow = clone(this.getItems[i]);
						season.children = clone([episode]);
						tvShow.children = clone([season]);
						// Ensure all nodes are open
						this.openDownloadPreviews.push(tvShow.id);
						this.openDownloadPreviews.push(season.id);
						break;
					}
				}
			}
			result.push(tvShow);
		}

		return result;
	}

	openDownloadConfirmationDialog(itemId: number, type: PlexMediaType): void {
		this.downloadPreview = this.createPreview(itemId, type);
		this.downloadPreviewtype = type;
		this.showDialog = true;
	}

	downloadTvShows(): void {
		let itemId = 0;
		if (this.downloadPreviewtype === 'TvShow') {
			itemId = this.downloadPreview[0].id;
		}

		if (this.downloadPreviewtype === 'Season') {
			itemId = this.downloadPreview[0].children[0].id;
		}

		if (this.downloadPreviewtype === 'Episode') {
			itemId = this.downloadPreview[0].children[0].children[0].id;
		}

		merge(
			// Setup progress bar
			SignalrService.getDownloadTaskCreationProgress().pipe(
				tap((data) => {
					this.progress = data;
					Log.debug(data);
				}),
				finalize(() => {
					this.showDialog = false;
					this.progress = null;
				}),
				takeWhile((data) => !data.isComplete),
			),
			// Download tvShows
			downloadTvShow(itemId, this.activeAccount?.id ?? 0, this.downloadPreviewtype).pipe(
				finalize(() => DownloadService.fetchDownloadList()),
			),
		).subscribe();
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
