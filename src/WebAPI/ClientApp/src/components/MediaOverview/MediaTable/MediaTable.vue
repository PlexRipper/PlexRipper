<template>
	<v-tree-view-table
		:items="items"
		:headers="getHeaders"
		:navigation="!hideNavigation"
		:open-all="detailMode"
		item-key="treeKeyId"
		load-children
		@selected="updateSelected"
		@download="downloadMedia"
		@load-children="getMedia"
	/>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { DownloadMediaDTO, DownloadTaskCreationProgress, PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import ITreeViewTableHeader from '@components/General/VTreeViewTable/ITreeViewTableHeader';
import ProgressComponent from '@components/Progress/ProgressComponent.vue';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import ITreeDownloadItem from '@mediaOverview/MediaTable/types/ITreeDownloadItem';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
	},
})
export default class MediaTable extends Vue {
	@Prop({ required: true, type: Array as () => PlexMediaDTO[] })
	readonly items!: PlexMediaDTO[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ required: false, type: Boolean })
	readonly hideNavigation!: boolean;

	@Prop({ type: Boolean })
	readonly detailMode!: boolean;

	@Prop({ required: true, type: Number })
	readonly libraryId!: number;

	expanded: string[] = [];

	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	visible: boolean[] = [];
	loadingButtons: string[] = [];

	selected: string[] = [];

	@Watch('items')
	updateVisible(): void {
		this.items.forEach(() => this.visible.push(false));
	}

	get containerRef(): any {
		return this.$refs.scrollbar;
	}

	isLoading(key: string): boolean {
		return this.loadingButtons.includes(key);
	}

	get getHeaders(): ITreeViewTableHeader[] {
		return [
			{
				text: 'Title',
				value: 'title',
				maxWidth: 250,
			},
			{
				text: 'Year',
				value: 'year',
				width: 50,
			},
			{
				text: 'Size',
				value: 'mediaSize',
				type: TreeViewTableHeaderEnum.FileSize,
				width: 100,
			},
			{
				text: 'Added At',
				value: 'addedAt',
				type: TreeViewTableHeaderEnum.Date,
				width: 100,
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
				type: TreeViewTableHeaderEnum.Date,
				width: 100,
			},
			{
				text: 'Actions',
				value: 'actions',
				type: TreeViewTableHeaderEnum.Actions,
				width: 100,
				sortable: false,
				defaultActions: ['download'],
			},
		];
	}

	createDownloadCommands(): DownloadMediaDTO[] {
		const downloads: DownloadMediaDTO[] = [];

		if (this.mediaType === PlexMediaType.TvShow) {
			const treeTvShows: ITreeDownloadItem[] = [];
			const episodesKeys: string[] = this.selected.filter((x) => x?.split('-')?.length === 3 ?? false);

			// Create a hierarchical tree of all selected media, TvShow -> Season -> Episode
			episodesKeys.forEach((x) => {
				const tvShowId = +x.split('-')[0];
				const seasonId = +x.split('-')[1];
				const episodeId = +x.split('-')[2];

				// Add tvShow to tree
				if (!treeTvShows.some((tvShow) => tvShow.id === tvShowId)) {
					treeTvShows.push({ id: tvShowId, children: [] });
				}

				// Add season to tree
				if (!treeTvShows.some((tvShow) => tvShow.children?.some((season) => season.id === seasonId))) {
					treeTvShows.find((x) => x.id === tvShowId)?.children?.push({ id: seasonId, children: [] });
				}

				// Add episode to tree
				treeTvShows
					.find((x) => x.id === tvShowId)
					?.children?.find((x) => x.id === seasonId)
					?.children?.push({ id: episodeId });
			});

			// Use the hierarchical tree and create downloadCommands.
			// The objective is to only return a seasonDownloadCommand, if all episodes have been selected
			// Instead of 10 episodeIds, we get 1 seasonId if all episodes in that season have been selected.
			// Same thing for a tvShowId, instead of multiple seasonIds, we get a single tvShowId if all seasons have been selected.
			const tvShowIds: number[] = [];
			let seasonIds: number[] = [];
			const episodesIds: number[] = [];
			treeTvShows.forEach((treeTvShow) => {
				// Find tvShow
				const tvShow = this.items.find((tvShow) => tvShow.id === treeTvShow.id);
				if (tvShow) {
					const tmpSeasonIds: number[] = [];
					treeTvShow.children?.forEach((treeSeason) => {
						// Find season
						const season = tvShow?.children?.find((x) => x.id === treeSeason.id);
						if (treeSeason?.children?.length === season?.children?.length) {
							tmpSeasonIds.push(treeSeason.id);
						} else {
							treeSeason.children?.forEach((x) => episodesIds.push(x.id));
						}
					});

					// Check if all seasons are checked, if so then add the TvShowId
					if (tvShow?.children?.length === tmpSeasonIds.length) {
						tvShowIds.push(treeTvShow.id);
						// If not then add the remaining to the seasonIds
					} else if (tmpSeasonIds.length > 0) {
						seasonIds = seasonIds.concat(tmpSeasonIds);
					}
				}
			});

			if (tvShowIds.length > 0) {
				downloads.push({
					mediaIds: tvShowIds,
					type: PlexMediaType.TvShow,
					plexAccountId: 0,
					libraryId: this.libraryId,
				});
			}

			if (seasonIds.length > 0) {
				downloads.push({
					mediaIds: seasonIds,
					type: PlexMediaType.Season,
					plexAccountId: 0,
					libraryId: this.libraryId,
				});
			}

			if (episodesIds.length > 0) {
				downloads.push({
					mediaIds: episodesIds,
					type: PlexMediaType.Episode,
					plexAccountId: 0,
					libraryId: this.libraryId,
				});
			}
		}

		if (this.mediaType === PlexMediaType.Movie) {
			downloads.push({
				mediaIds: this.selected?.map((x) => +x),
				type: PlexMediaType.Movie,
				plexAccountId: 0,
				libraryId: this.libraryId,
			});
		}

		return downloads;
	}

	updateSelected(selected: string[]): void {
		this.selected = selected;
		this.$emit('selected', selected);
	}

	async downloadMedia(item: PlexMediaDTO): Promise<void> {
		// Set as currently loading.
		this.loadingButtons.push(item.key.toString());
		const downloadCommand: DownloadMediaDTO = {
			type: item.type,
			mediaIds: [],
			plexAccountId: 0,
			libraryId: this.libraryId,
		};
		switch (item.type) {
			case PlexMediaType.Movie:
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.TvShow:
				if (item.children?.length === 0) {
					await new Promise((resolve) => this.getMedia({ item, resolve }));
				}
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.Season:
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.Episode:
				downloadCommand.mediaIds.push(item.id);
				break;
			default:
				return;
		}
		// Set finished loading
		const i = this.loadingButtons.findIndex((x) => x === item.key.toString());
		if (i > -1) {
			this.loadingButtons.splice(i, 1);
		} else {
			this.loadingButtons = [];
		}
		Log.info('download', downloadCommand);
		this.$emit('download', [downloadCommand]);
	}

	/*
A promise is send to the parent, which will resolve once the data is available. After which the node expands.
 */
	getMedia(payload: { item: any; resolve: Function }): void {
		this.$emit('request-media', payload);
	}
}
</script>
