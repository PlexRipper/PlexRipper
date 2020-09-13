<template>
	<v-row justify="center">
		<v-col cols="12">
			<media-table
				:headers="getHeaders"
				:items="getItems"
				:media-type="getType"
				@download="openDownloadConfirmationDialog($event.itemId, $event.type)"
			/>
		</v-col>
		<!-- The "Are you sure" dialog -->
		<v-col cols="12">
			<v-dialog v-model="showDialog" :max-width="500" scrollable>
				<v-card v-if="progress === null">
					<v-card-title> Are you sure? </v-card-title>
					<v-card-subtitle>
						<p>Plex Ripper will start downloading all of the following:</p>
					</v-card-subtitle>
					<!-- Show Download Task Preview -->
					<v-card-text>
						<v-treeview :items="downloadPreview" item-text="title" :open.sync="openDownloadPreviews" open-all></v-treeview>
					</v-card-text>
					<v-divider></v-divider>

					<v-card-actions>
						<v-btn large @click="showDialog = false"> Cancel </v-btn>
						<v-spacer></v-spacer>
						<v-btn color="success" large @click="downloadTvShows()"> Yes! </v-btn>
					</v-card-actions>
				</v-card>

				<!-- Download Task Creation Progressbar -->
				<v-card v-else>
					<v-card-title class="justify-center">
						Creating download tasks {{ progress.current }} of {{ progress.total }}
					</v-card-title>
					<v-card-text>
						<progress-component :percentage="progress.percentage" text="" />
					</v-card-text>
				</v-card>
			</v-dialog>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import DownloadService from '@service/downloadService';
import type { PlexAccountDTO } from '@dto/mainApi';
import { DownloadTaskCreationProgress, PlexMediaType, PlexTvShowDTO } from '@dto/mainApi';
import { downloadTvShow } from '@api/plexDownloadApi';
import { clone } from 'lodash';
import { catchError, finalize, takeWhile, tap } from 'rxjs/operators';
import SignalrService from '@service/signalrService';
import { merge, of } from 'rxjs';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/iTreeViewItem';
import ProgressComponent from '@components/ProgressComponent.vue';

@Component({
	components: {
		MediaTable,
		ProgressComponent,
	},
})
export default class TVShowsTable extends Vue {
	@Prop({ required: true, type: Object as () => PlexAccountDTO })
	readonly activeAccount!: PlexAccountDTO;

	@Prop({ required: true, type: Array as () => PlexTvShowDTO[] })
	readonly tvShows!: PlexTvShowDTO[];

	expanded: string[] = [];
	singleExpand: boolean = false;
	selected: number[] = [];

	showDialog: boolean = false;
	downloadPreview: ITreeViewItem[] = [];
	downloadPreviewType: PlexMediaType = PlexMediaType.None;
	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	get getType(): PlexMediaType {
		return PlexMediaType.TvShow;
	}

	get getItems(): ITreeViewItem[] {
		const items: ITreeViewItem[] = [];
		this.tvShows.forEach((tvShow: PlexTvShowDTO) => {
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
								title: episode.title ?? '',
								type: PlexMediaType.Episode,
								children: [],
								item: episode,
								addedAt: episode.addedAt ?? '',
								updatedAt: episode.updatedAt ?? '',
							});
						});
						// Add seasons
						seasons.push({
							id: season.id,
							key: `${tvShow.id}-${season.id}`,
							title: season.title ?? '',
							type: PlexMediaType.Season,
							children: episodes,
							item: season,
							addedAt: season.addedAt ?? '',
							updatedAt: season.updatedAt ?? '',
						});
					}
				});
				// Add tvShow
				items.push({
					id: tvShow.id,
					key: `${tvShow.id}`,
					title: tvShow.title ?? '',
					year: tvShow.year,
					type: PlexMediaType.TvShow,
					item: tvShow,
					children: seasons,
					addedAt: tvShow.addedAt ?? '',
					updatedAt: tvShow.updatedAt ?? '',
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
				width: 400,
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
		];
	}

	get getLeafs(): number[] {
		return this.getItems.map((x) => x.children?.map((y) => y.children?.map((z) => z.id))).flat(2);
	}

	createPreview(itemId: number, type: PlexMediaType): ITreeViewItem[] {
		const result: ITreeViewItem[] = [];
		this.openDownloadPreviews = [];

		// Tv show: Show tvShow -> with all seasons -> with all episodes
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

		// Season: Show tvShow -> season -> with all episodes
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

		// Episode: Show tvShow -> season -> episode without anything else
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
		this.downloadPreviewType = type;
		this.showDialog = true;
	}

	downloadTvShows(): void {
		let itemId = 0;
		if (this.downloadPreviewType === 'TvShow') {
			itemId = this.downloadPreview[0].id;
		}

		if (this.downloadPreviewType === 'Season') {
			itemId = this.downloadPreview[0].children[0].id;
		}

		if (this.downloadPreviewType === 'Episode') {
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
				catchError(() => {
					return of(undefined);
				}),
			),
			// Download tvShows
			downloadTvShow(itemId, this.activeAccount?.id ?? 0, this.downloadPreviewType).pipe(
				finalize(() => DownloadService.fetchDownloadList()),
				catchError(() => {
					return of(undefined);
				}),
			),
		)
			.pipe(
				catchError(() => {
					this.showDialog = false;
					this.progress = null;
					return of(false);
				}),
			)
			.subscribe();
	}
}
</script>
