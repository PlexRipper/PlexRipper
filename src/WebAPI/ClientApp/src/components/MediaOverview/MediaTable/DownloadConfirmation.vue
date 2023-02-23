<template>
	<!-- The "Are you sure" dialog -->
	<v-col cols="12">
		<v-dialog v-model="showDialog" :max-width="500" scrollable>
			<v-card v-if="isConfirmationEnabled && progress === null">
				<v-card-title> {{ $t('components.download-confirmation.header') }}</v-card-title>
				<v-card-subtitle class="py-2">
					<span>{{ $t('components.download-confirmation.description') }}</span> <br />
					<span>{{ $t('components.download-confirmation.total-size') }}<file-size :size="totalSize" /></span>
				</v-card-subtitle>
				<v-divider />
				<!-- Show Download Task Preview -->
				<v-card-text class="pa-0" style="min-height: 60vh; max-height: 60vh">
					<vue-scroll ref="previewscrollbar" :ops="{ scrollPanel: { scrollingX: false } }">
						<v-col cols="12" class="px-2 py-0">
							<v-treeview :items="downloadPreview" item-text="title" item-key="key" :open="getLeafs" open-all>
								<template #prepend="{ item }">
									<media-type-icon :media-type="item.type" />
								</template>
								<template #append="{ item }">
									(
									<file-size :size="item.mediaSize" />
									)
								</template>
							</v-treeview>
						</v-col>
					</vue-scroll>
				</v-card-text>
				<v-divider />

				<v-card-actions>
					<CancelButton @click="showDialog = false" />
					<v-spacer></v-spacer>
					<ConfirmButton @click="confirmDownload()" />
				</v-card-actions>
			</v-card>
		</v-dialog>
	</v-col>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { SettingsService } from '@service';

@Component
export default class DownloadConfirmation extends Vue {
	@Prop({ required: true, type: Array as () => PlexMediaDTO[] })
	readonly items!: PlexMediaDTO[];

	askDownloadMovieConfirmation: boolean = false;
	askDownloadTvShowConfirmation: boolean = false;
	askDownloadSeasonConfirmation: boolean = false;
	askDownloadEpisodeConfirmation: boolean = false;

	showDialog: boolean = false;
	downloadPreview: PlexMediaDTO[] = [];
	downloadMediaCommand: DownloadMediaDTO[] = [];

	get isConfirmationEnabled(): boolean {
		if (this.downloadMediaCommand.length === 1) {
			switch (this.downloadMediaCommand[0].type) {
				case PlexMediaType.Movie:
					return this.askDownloadMovieConfirmation;
				case PlexMediaType.TvShow:
					return this.askDownloadTvShowConfirmation;
				case PlexMediaType.Season:
					return this.askDownloadSeasonConfirmation;
				case PlexMediaType.Episode:
					return this.askDownloadEpisodeConfirmation;
				default:
					return true;
			}
		}
		return true;
	}

	get totalSize(): number {
		let size = 0;
		if (this.downloadPreview.length > 0) {
			this.downloadPreview.forEach((x) => (size += x.mediaSize ?? 0));
		}
		return size;
	}

	private createPreview(downloadMediaCommands: DownloadMediaDTO[]): void {
		let downloadPreview: PlexMediaDTO[] = [];

		const movieDownloadCommand = downloadMediaCommands.find((x) => x.type === PlexMediaType.Movie);
		// If statements instead of switch to avoid having to overcomplicate the variable names.
		// Movie: Show only the movie
		if (movieDownloadCommand) {
			downloadPreview = downloadPreview.concat(
				this.items.filter((movie) => movieDownloadCommand.mediaIds.includes(movie.id)),
			);
		}

		// TvShow: Show tvShow -> with all season -> with all episodes
		const tvShowDownloadCommand = downloadMediaCommands.find((x) => x.type === PlexMediaType.TvShow);
		if (tvShowDownloadCommand) {
			downloadPreview = downloadPreview.concat(
				this.items.filter((tvShow) => tvShowDownloadCommand.mediaIds.includes(tvShow.id)),
			);
		}

		// Season: Show tvShow -> season -> with all episodes
		const tvShowSeasonDownloadCommand = downloadMediaCommands.find((x) => x.type === PlexMediaType.Season);
		if (tvShowSeasonDownloadCommand) {
			const mediaIds = tvShowSeasonDownloadCommand.mediaIds;

			downloadPreview = downloadPreview.concat(
				this.items
					.filter((tvShow) => tvShow.children?.some((season) => mediaIds.includes(season.id)))
					.map((tvShow) => {
						return {
							...tvShow,
							children: tvShow.children?.filter((season) => mediaIds.includes(season.id)),
						};
					}),
			);
		}

		// Episode: Show tvShow -> season -> episode without anything else
		const tvShowEpisodeDownloadCommand = downloadMediaCommands.find((x) => x.type === PlexMediaType.Episode);
		if (tvShowEpisodeDownloadCommand) {
			const mediaIds = tvShowEpisodeDownloadCommand.mediaIds;
			const filterResult = this.items
				.filter((tvShow) =>
					tvShow.children?.some((season) => season.children?.some((episode) => mediaIds.includes(episode.id))),
				)
				.map((tvShow) => {
					// Create the tvShow
					return {
						...tvShow,
						children: tvShow.children
							?.filter((season: PlexMediaDTO) => season?.children?.some((episode) => mediaIds.includes(episode.id)))
							.map((season: PlexMediaDTO) => {
								// Create the tvShowSeason
								return {
									...season,
									children: season?.children?.filter((episode) => mediaIds.includes(episode.id)),
								};
							}),
					};
				});

			// Merge the tvShows
			filterResult.forEach((filterResultTvShow) => {
				const downloadPreviewTvShow = downloadPreview.find((x) => x.id === filterResultTvShow.id);
				if (downloadPreviewTvShow) {
					// There already is a tvShow in the filterResult with the same id
					filterResultTvShow.children?.forEach((season) => {
						const filterResultTvShowSeason = downloadPreviewTvShow?.children?.find((x) => x.id === season.id);
						if (!filterResultTvShowSeason) {
							downloadPreviewTvShow?.children?.push(season);
						}
					});
				} else {
					downloadPreview.push(filterResultTvShow);
				}
			});
		}

		// Calculate mediaSize for each parent and child (TvShow and Season);

		downloadPreview.forEach((parent) => {
			if (parent.type === PlexMediaType.TvShow || parent.type === PlexMediaType.Season) {
				parent.children?.forEach((child) => {
					child.mediaSize = child?.children?.map((x) => x.mediaSize).sum() ?? 0;
				});
				parent.mediaSize = parent.children?.map((x) => x.mediaSize).sum() ?? 0;
			}
		});

		Log.info('downloadPreview', downloadPreview);
		this.downloadPreview = downloadPreview;
	}

	/* Recursively retrieve all unique keys used in the items: ITreeViewItem[] */
	get getLeafs(): string[] {
		let keys: string[] = [];
		keys = keys.concat(this.downloadPreview.map((x) => x.key.toString()));
		keys = keys.concat(this.downloadPreview.map((x) => x.children?.map((y) => y.key.toString()) ?? [])?.flat(1) ?? []);
		keys = keys.concat(
			this.downloadPreview
				.map((x) => x.children?.map((y) => y.children?.map((z) => z.key.toString()) ?? []) ?? [])
				?.flat(2) ?? [],
		);
		keys = keys.concat(
			this.downloadPreview
				.map(
					(x) =>
						x.children?.map((y) => y.children?.map((z) => z.children?.map((w) => w.key.toString()) ?? []) ?? []) ??
						[],
				)
				?.flat(3),
		);
		return keys;
	}

	openDialog(downloadMediaCommand: DownloadMediaDTO[]): void {
		this.downloadMediaCommand = downloadMediaCommand;
		Log.info('downloadMedia', downloadMediaCommand);
		if (this.isConfirmationEnabled) {
			this.createPreview(downloadMediaCommand);
		} else {
			this.confirmDownload();
		}
		this.showDialog = true;
	}

	closeDialog(): void {
		this.showDialog = false;
		this.downloadPreview = [];
	}

	confirmDownload(): void {
		this.$emit('download', this.downloadMediaCommand);
		this.showDialog = false;
	}

	mounted(): void {
		useSubscription(
			SettingsService.getAskDownloadMovieConfirmation().subscribe((value) => {
				this.askDownloadMovieConfirmation = value;
			}),
		);
		useSubscription(
			SettingsService.getAskDownloadTvShowConfirmation().subscribe((value) => {
				this.askDownloadTvShowConfirmation = value;
			}),
		);
		useSubscription(
			SettingsService.getAskDownloadSeasonConfirmation().subscribe((value) => {
				this.askDownloadSeasonConfirmation = value;
			}),
		);
		useSubscription(
			SettingsService.getAskDownloadEpisodeConfirmation().subscribe((value) => {
				this.askDownloadEpisodeConfirmation = value;
			}),
		);
	}
}
</script>
