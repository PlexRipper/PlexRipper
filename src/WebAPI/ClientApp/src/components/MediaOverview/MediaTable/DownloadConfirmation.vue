<template>
	<!-- The "Are you sure" dialog -->
	<v-col cols="12">
		<v-dialog v-model="showDialog" :max-width="500" scrollable>
			<v-card v-if="isConfirmationEnabled && progress === null">
				<v-card-title> Are you sure? </v-card-title>
				<v-card-subtitle>
					<p>Plex Ripper will start downloading the following:</p>
				</v-card-subtitle>
				<!-- Show Download Task Preview -->
				<v-card-text>
					<perfect-scrollbar>
						<v-treeview :items="downloadPreview" item-text="title" item-key="key" :open="getLeafs" open-all>
							<template #prepend="{ item }">
								<v-icon>
									{{ item.type | mediaTypeIcon }}
								</v-icon>
							</template>
						</v-treeview>
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
import { settingsStore as SettingsStore } from '@/store';
import ButtonType from '@enums/buttonType';

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
	downloadMediaCommand: DownloadMediaDTO | null = null;

	get isConfirmationEnabled(): boolean {
		switch (this.downloadMediaCommand?.type) {
			case PlexMediaType.Movie:
				return SettingsStore.askDownloadMovieConfirmation;
			case PlexMediaType.TvShow:
				return SettingsStore.askDownloadTvShowConfirmation;
			case PlexMediaType.Season:
				return SettingsStore.askDownloadSeasonConfirmation;
			case PlexMediaType.Episode:
				return SettingsStore.askDownloadEpisodeConfirmation;
			default:
				return true;
		}
	}

	get cancelButtonType(): ButtonType {
		return ButtonType.Cancel;
	}

	get confirmButtonType(): ButtonType {
		return ButtonType.Confirm;
	}

	private createPreview(downloadMediaCommand: DownloadMediaDTO): void {
		let filterResult: ITreeViewItem[] = [];
		const mediaIds = downloadMediaCommand.mediaIds;

		// If statements instead of switch to avoid having to overcomplicate the variable names.

		// Movie: Show only the movie
		if (downloadMediaCommand.type === PlexMediaType.Movie) {
			filterResult = this.items.filter((x) => mediaIds.some((y) => y === x.id)) ?? [];
		}

		// Episode: Show tvShow -> season -> episode without anything else
		// First filter and then map to exclude unneeded tvshows/seasons/episodes
		if (
			downloadMediaCommand.type === PlexMediaType.TvShow ||
			downloadMediaCommand.type === PlexMediaType.Season ||
			downloadMediaCommand.type === PlexMediaType.Episode
		) {
			filterResult = this.items
				.filter((tvShow) => tvShow.children?.some((season) => season.children?.some((z) => mediaIds.some((y) => y === z.id))))
				.map((tvShow) => {
					return {
						...tvShow,
						children:
							tvShow?.children ??
							[]
								.filter((season: ITreeViewItem) => season?.children?.some((episode) => mediaIds.some((y) => y === episode.id)))
								.map((season: ITreeViewItem) => {
									return {
										...season,
										children: season?.children?.filter((episode) => mediaIds.some((y) => y === episode.id)),
									};
								}),
					};
				});
		}

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

	openDialog(downloadMediaCommand: DownloadMediaDTO): void {
		this.downloadMediaCommand = downloadMediaCommand;
		if (this.isConfirmationEnabled) {
			this.createPreview(downloadMediaCommand);
		} else {
			this.confirmDownload();
		}
		this.showDialog = true;
	}

	closeDialog(): void {
		this.showDialog = false;
	}

	confirmDownload(): void {
		this.$emit('download', this.downloadMediaCommand);
	}
}
</script>
