<template>
	<v-row justify="center" class="media-table flex-nowrap" no-gutters>
		<v-col>
			<!-- Table Headers -->
			<v-row class="media-table-header">
				<!-- Checkbox -->
				<v-col class="ml-6 select-all-check" style="max-width: 50px">
					<v-checkbox :indeterminate="isIndeterminate" color="red" @change="selectAll($event)"></v-checkbox>
				</v-col>
				<!-- Title -->
				<v-col cols="4" class="title-column">
					{{ getHeaders[0].text }}
				</v-col>
				<v-spacer />
				<!-- Other columns -->
				<v-col v-for="(header, i) in getHeaders.slice(1, getHeaders.length)" :key="i" cols="auto">
					<v-sheet :width="header.width" :max-width="header.width" class="no-background">{{ header.text }}</v-sheet>
				</v-col>
				<!-- Actions -->
				<v-col cols="auto" class="text-center">
					<v-sheet width="70" class="no-background">Actions</v-sheet>
				</v-col>
			</v-row>
			<!-- TreeView Table -->
			<v-row no-gutters :class="['media-table-content', detailMode ? 'detail-mode' : '']">
				<perfect-scrollbar ref="scrollbarmediatable" :options="{ suppressScrollX: true }">
					<v-col id="media-table-body" class="col px-0">
						<template v-for="(parentItem, i) in items">
							<v-lazy
								:key="i"
								:options="{
									threshold: 0.25,
								}"
								:min-height="50"
								:data-title="parentItem.title"
								transition="scroll-x-reverse-transition"
							>
								<v-treeview
									selectable
									selected-color="red"
									selection-type="leaf"
									hoverable
									expand-icon="mdi-chevron-down"
									:items="[parentItem]"
									:load-children="getMedia"
									:open-all="detailMode"
									transition
									item-key="key"
									item-text="title"
									class="media-table-row"
									@input="updateSelected(i, $event)"
								>
									<template #label="{ item }">
										<v-row class="media-table-content-row" align="center">
											<!-- Title -->
											<v-col cols="4" class="title-column">{{ item[getHeaders[0].value] }}</v-col>
											<v-spacer />
											<!-- Other columns -->
											<v-col v-for="(header, index) in getHeaders.slice(1, getHeaders.length)" :key="index" cols="auto">
												<v-sheet :width="header.width" :max-width="header.width" class="no-background">
													<date-time v-if="header.type === 'date'" :text="item[header.value]" :time="false" short-date />
													<file-size v-else-if="header.type === 'data'" :size="item[header.value]" />
													<span v-else>{{ item[header.value] }}</span>
												</v-sheet>
											</v-col>
											<!-- Actions -->
											<v-col cols="auto" class="py-0">
												<v-sheet width="70" class="no-background text-center">
													<p-btn
														button-type="download"
														:loading="isLoading(item.key)"
														icon-mode
														@click="downloadMedia(item)"
													></p-btn>
												</v-sheet>
											</v-col>
										</v-row>
									</template>
								</v-treeview>
							</v-lazy>
						</template>
					</v-col>
				</perfect-scrollbar>
			</v-row>
		</v-col>
		<alphabet-navigation v-if="!hideNavigation" :items="items" container-ref="scrollbarmediatable" />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { DownloadMediaDTO, DownloadTaskCreationProgress, PlexMediaType } from '@dto/mainApi';
import ITreeViewTableHeader from '@components/General/VTreeViewTable/ITreeViewTableHeader';
import ProgressComponent from '@components/ProgressComponent.vue';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';
import ITreeDownloadItem from '@mediaOverview/MediaTable/types/ITreeDownloadItem';
import ITreeViewItem from './types/ITreeViewItem';
import { settingsStore } from '~/store';

declare interface ISelection {
	index: number;
	keys: string[];
}

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
		AlphabetNavigation,
	},
})
export default class MediaTable extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ required: false, type: Boolean })
	readonly hideNavigation!: boolean;

	@Prop({ type: Boolean })
	readonly detailMode!: boolean;

	@Prop({ required: true, type: Number })
	readonly libraryId!: number;

	selected: ISelection[] = [];

	expanded: string[] = [];

	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	visible: boolean[] = [];
	loadingButtons: string[] = [];

	@Watch('items')
	updateVisible(): void {
		this.items.forEach(() => this.visible.push(false));
	}

	get getSelected(): string[] {
		return this.selected.map((x) => x.keys).flat(1);
	}

	get isIndeterminate(): boolean {
		return this.getSelected.length !== this.selected.length && this.selected.length > 0;
	}

	get containerRef(): any {
		return this.$refs.scrollbar;
	}

	get activeAccountId(): number {
		return settingsStore.activeAccountId;
	}

	retrieveAllLeafs(items: ITreeViewItem[]): string[] {
		return (
			items
				.map((root) => root.children?.map((child1) => child1.children?.map((child2) => child2.key) ?? child1.key) ?? root.key)
				?.flat(2) ?? []
		);
	}

	isLoading(key: string): boolean {
		return this.loadingButtons.some((x) => x === key);
	}

	updateSelected(i: number, selected: string[]) {
		const index = this.selected.findIndex((x) => x.index === i);
		if (index === -1) {
			this.selected.push({ index: i, keys: selected });
		} else {
			this.selected.splice(index, 1, { index: i, keys: selected });
		}
		this.emitSelected();
	}

	selectAll(state: boolean): void {
		if (state) {
			this.items.forEach((x, i) => {
				this.selected.addOrReplace(i, { index: i, keys: this.retrieveAllLeafs([x]) } as ISelection);
			});
		} else {
			this.selected = [];
		}
		this.emitSelected();
	}

	emitSelected(): void {
		this.$emit('selected', this.getSelected);
	}

	get getHeaders(): ITreeViewTableHeader[] {
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
				text: 'Size',
				value: 'mediaSize',
				width: 100,
				type: TreeViewTableHeaderEnum.FileSize,
			},
			{
				text: 'Added At',
				value: 'addedAt',
				width: 150,
				type: TreeViewTableHeaderEnum.Date,
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
				width: 150,
				type: TreeViewTableHeaderEnum.Date,
			},
		];
	}

	createDownloadCommands(): DownloadMediaDTO[] {
		const downloads: DownloadMediaDTO[] = [];
		const selected = this.getSelected;

		if (this.mediaType === PlexMediaType.TvShow) {
			const treeTvShows: ITreeDownloadItem[] = [];
			const episodesKeys: string[] = selected.filter((x) => x?.split('-')?.length === 3 ?? false);

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
					plexAccountId: this.activeAccountId,
					libraryId: this.libraryId,
				});
			}

			if (seasonIds.length > 0) {
				downloads.push({
					mediaIds: seasonIds,
					type: PlexMediaType.Season,
					plexAccountId: this.activeAccountId,
					libraryId: this.libraryId,
				});
			}

			if (episodesIds.length > 0) {
				downloads.push({
					mediaIds: episodesIds,
					type: PlexMediaType.Episode,
					plexAccountId: this.activeAccountId,
					libraryId: this.libraryId,
				});
			}
		}

		if (this.mediaType === PlexMediaType.Movie) {
			downloads.push({
				mediaIds: selected.map((x) => +x),
				type: PlexMediaType.Movie,
				plexAccountId: this.activeAccountId,
				libraryId: this.libraryId,
			});
		}

		return downloads;
	}

	/*
	A promise is send to the parent, which will resolve once the data is available. After which the node expands.
	 */
	getMedia(item: ITreeViewItem): Promise<ITreeViewItem> {
		return new Promise((resolve) => this.$emit('request-media', { mediaId: item.id, resolve }));
	}

	async downloadMedia(item: ITreeViewItem): Promise<void> {
		// Set as currently loading.
		this.loadingButtons.push(item.key);
		const downloadCommand: DownloadMediaDTO = {
			type: item.type,
			mediaIds: [],
			plexAccountId: this.activeAccountId,
			libraryId: this.libraryId,
		};
		switch (item.type) {
			case PlexMediaType.Movie:
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.TvShow:
				if (item.children?.length === 0) {
					await this.getMedia(item);
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
		const i = this.loadingButtons.findIndex((x) => x === item.key);
		if (i > -1) {
			this.loadingButtons.splice(i, 1);
		} else {
			this.loadingButtons = [];
		}
		this.$emit('download', [downloadCommand]);
	}
}
</script>
