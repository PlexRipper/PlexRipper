<template>
	<!-- The "Are you sure" dialog -->
	<v-col cols="12">
		<v-dialog v-model="showDialog" :max-width="500" scrollable>
			<v-card v-if="isConfirmationEnabled && progress === null">
				<v-card-title> Are you sure? </v-card-title>
				<v-card-subtitle class="py-2">
					<span>Plex Ripper will start downloading the following:</span> <br />
					<span>Total size: <file-size :size="totalSize" /></span>
				</v-card-subtitle>
				<v-divider />
				<!-- Show Download Task Preview -->
				<v-card-text class="pa-0" style="min-height: 60vh; max-height: 60vh">
					<perfect-scrollbar ref="previewscrollbar" :options="{ suppressScrollX: true }">
						<v-col cols="12" class="px-2 py-0">
							<v-treeview :items="downloadPreview" item-text="title" item-key="key" :open="getLeafs" open-all>
								<template #prepend="{ item }">
									<v-icon>
										{{ item.type | mediaTypeIcon }}
									</v-icon>
								</template>
								<template #append="{ item }"> (<file-size :size="item.mediaSize" />) </template>
							</v-treeview>
						</v-col>
					</perfect-scrollbar>
				</v-card-text>
				<v-divider></v-divider>

				<v-card-actions>
					<p-btn :button-type="cancelButtonType" @click="showDialog = false" />
					<v-spacer></v-spacer>
					<p-btn :button-type="confirmButtonType" @click="confirmDownload()" />
				</v-card-actions>
			</v-card>

			<!-- Download Task Creation Progressbar -->
			<v-card v-if="progress">
				<v-card-title class="justify-center">
					Creating download tasks {{ progress.current }} of {{ progress.total }}
				</v-card-title>
				<v-card-text>
					<progress-component :percentage="progress.percentage" text="" />
				</v-card-text>
			</v-card>
		</v-dialog>
	</v-col>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import ProgressComponent from '@components/ProgressComponent.vue';
import { DownloadMediaDTO, DownloadTaskCreationProgress, PlexMediaType } from '@dto/mainApi';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import { settingsStore } from '@/store';
import ButtonType from '@enums/buttonType';
import Log from 'consola';

@Component({
	components: {
		ProgressComponent,
	},
})
export default class DownloadConfirmation extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true })
	readonly progress!: DownloadTaskCreationProgress | null;

	showDialog: boolean = false;
	downloadPreview: ITreeViewItem[] = [];
	downloadMediaCommand: DownloadMediaDTO[] = [];

	get isConfirmationEnabled(): boolean {
		if (this.downloadMediaCommand.length === 1) {
			switch (this.downloadMediaCommand[0].type) {
				case PlexMediaType.Movie:
					return settingsStore.askDownloadMovieConfirmation;
				case PlexMediaType.TvShow:
					return settingsStore.askDownloadTvShowConfirmation;
				case PlexMediaType.Season:
					return settingsStore.askDownloadSeasonConfirmation;
				case PlexMediaType.Episode:
					return settingsStore.askDownloadEpisodeConfirmation;
				default:
					return true;
			}
		}
		return true;
	}

	get cancelButtonType(): ButtonType {
		return ButtonType.Cancel;
	}

	get confirmButtonType(): ButtonType {
		return ButtonType.Confirm;
	}

	get totalSize(): number {
		let size = 0;
		if (this.downloadPreview.length > 0) {
			this.downloadPreview.forEach((x) => (size += x.mediaSize ?? 0));
		}
		return size;
	}

	private createPreview(downloadMediaCommands: DownloadMediaDTO[]): void {
		let filterResult: ITreeViewItem[] = [];

		// If statements instead of switch to avoid having to overcomplicate the variable names.
		downloadMediaCommands.forEach((downloadMediaCommand) => {
			const mediaIds = downloadMediaCommand.mediaIds;

			// Movie: Show only the movie
			if (downloadMediaCommand.type === PlexMediaType.Movie) {
				filterResult = filterResult.concat(this.items.filter((movie) => mediaIds.includes(movie.id)));
			}

			// TvShow: Show tvShow -> with all season -> with all episodes
			if (downloadMediaCommand.type === PlexMediaType.TvShow) {
				filterResult = filterResult.concat(this.items.filter((tvShow) => mediaIds.some((x) => x === tvShow.id)));
			}

			// Season: Show tvShow -> season -> with all episodes
			if (downloadMediaCommand.type === PlexMediaType.Season) {
				filterResult = filterResult.concat(
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
			if (downloadMediaCommand.type === PlexMediaType.Episode) {
				filterResult = filterResult.concat(
					this.items
						.filter((tvShow) =>
							tvShow.children?.some((season) => season.children?.some((episode) => mediaIds.includes(episode.id))),
						)
						.map((tvShow) => {
							return {
								...tvShow,
								children: tvShow.children
									?.filter((season: ITreeViewItem) => season?.children?.some((episode) => mediaIds.includes(episode.id)))
									.map((season: ITreeViewItem) => {
										return {
											...season,
											children: season?.children?.filter((episode) => mediaIds.includes(episode.id)),
										};
									}),
							};
						}),
				);
			}
		});

		this.downloadPreview = filterResult;
	}

	/* Recursively retrieve all unique keys used in the items: ITreeViewItem[] */
	get getLeafs(): string[] {
		let keys: string[] = [];
		keys = keys.concat(this.downloadPreview.map((x) => x.key));
		keys = keys.concat(this.downloadPreview.map((x) => x.children?.map((y) => y.key) ?? [])?.flat(1) ?? []);
		keys = keys.concat(
			this.downloadPreview.map((x) => x.children?.map((y) => y.children?.map((z) => z.key) ?? []) ?? [])?.flat(2) ?? [],
		);
		keys = keys.concat(
			this.downloadPreview
				.map((x) => x.children?.map((y) => y.children?.map((z) => z.children?.map((w) => w.key) ?? []) ?? []) ?? [])
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
	}
}
</script>
