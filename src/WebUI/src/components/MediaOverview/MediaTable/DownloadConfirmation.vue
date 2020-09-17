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
					<v-treeview
						:items="downloadPreview"
						item-text="title"
						item-key="key"
						:open.sync="openDownloadPreviews"
						open-all
					></v-treeview>
				</v-card-text>
				<v-divider></v-divider>

				<v-card-actions>
					<v-btn large @click="showDialog = false"> Cancel </v-btn>
					<v-spacer></v-spacer>
					<v-btn color="success" large @click="confirmDownload()"> Yes! </v-btn>
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
import { DownloadTaskCreationProgress, PlexMediaType } from '@dto/mainApi';
import ITreeViewItem from '@mediaOverview/MediaTable/types/iTreeViewItem';
import { clone } from 'lodash';
import IMediaId from '@mediaOverview/MediaTable/types/IMediaId';
import { settingsStore as SettingsStore } from '@/store';

@Component<DownloadConfirmation>({
	components: {
		ProgressComponent,
	},
})
export default class DownloadConfirmation extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true })
	readonly progress!: DownloadTaskCreationProgress | null;

	openDownloadPreviews: string[] = [];
	downloadPreview: ITreeViewItem[] = [];
	showDialog: boolean = false;
	mediaId: IMediaId = { id: 0, type: PlexMediaType.None };

	get isConfirmationEnabled(): boolean {
		switch (this.mediaId.type) {
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

	private createPreview(mediaId: IMediaId): void {
		const result: ITreeViewItem[] = [];
		this.openDownloadPreviews = [];

		// If statements instead of switch to avoid having to overcomplicate the variable names.

		// Movie: Show only the movie
		if (mediaId.type === PlexMediaType.Movie) {
			const movie: ITreeViewItem | undefined = this.items.find((x) => x.id === mediaId.id);
			if (movie) {
				// Ensure all nodes are open
				this.openDownloadPreviews.push(movie.key);
				result.push(clone(movie));
			}
		}

		// Tv show: Show tvShow -> with all seasons -> with all episodes
		if (mediaId.type === PlexMediaType.TvShow) {
			const tvShow: ITreeViewItem | undefined = this.items.find((x) => x.id === mediaId.id);
			if (tvShow) {
				// Ensure all nodes are open
				this.getLeafs([tvShow]).forEach((x) => this.openDownloadPreviews.push(x));
				result.push(clone(tvShow));
			}
		}

		// Season: Show tvShow -> season -> with all episodes
		if (mediaId.type === PlexMediaType.Season) {
			let tvShow: ITreeViewItem = {} as ITreeViewItem;
			for (let i = 0; i < this.items.length; i++) {
				const season = this.items[i].children.find((c) => c.id === mediaId.id);
				if (season) {
					tvShow = clone(this.items[i]);
					tvShow.children = clone([season]);
					// Ensure all nodes are open
					this.openDownloadPreviews.push(tvShow.key);
					this.openDownloadPreviews.push(season.key);
					break;
				}
			}
			result.push(tvShow);
		}

		// Episode: Show tvShow -> season -> episode without anything else
		if (mediaId.type === PlexMediaType.Episode) {
			let tvShow: ITreeViewItem = {} as ITreeViewItem;
			for (let i = 0; i < this.items.length; i++) {
				for (let j = 0; j < this.items[i].children.length; j++) {
					const season: ITreeViewItem = clone(this.items[i].children[j]);
					const episode: ITreeViewItem | undefined = season?.children?.find((c) => c.id === mediaId.id);
					if (episode) {
						tvShow = clone(this.items[i]);
						season.children = clone([episode]);
						tvShow.children = clone([season]);
						// Ensure all nodes are open
						this.openDownloadPreviews.push(tvShow.key);
						this.openDownloadPreviews.push(season.key);
						break;
					}
				}
			}
			result.push(tvShow);
		}
		this.downloadPreview = result;
	}

	/* Recursively retrieve all unique keys used in the items: ITreeViewItem[] */
	getLeafs(items: ITreeViewItem[]): string[] {
		let keys: string[] = [];
		keys = keys.concat(items.map((x) => x.key));
		keys = keys.concat(items.map((x) => x.children?.map((y) => y.key)).flat(1));
		keys = keys.concat(items.map((x) => x.children?.map((y) => y.children?.map((z) => z.key))).flat(2));
		keys = keys.concat(items.map((x) => x.children?.map((y) => y.children?.map((z) => z.children?.map((w) => w.key)))).flat(3));
		return keys;
	}

	openDialog(mediaId: IMediaId): void {
		this.mediaId = mediaId;
		if (this.isConfirmationEnabled) {
			this.createPreview(mediaId);
		} else {
			this.confirmDownload();
		}
		this.showDialog = true;
	}

	closeDialog(): void {
		this.showDialog = false;
	}

	confirmDownload(): void {
		this.$emit('download', this.mediaId);
	}
}
</script>
