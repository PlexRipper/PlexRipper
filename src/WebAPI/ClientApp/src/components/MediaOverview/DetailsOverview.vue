<template>
	<page v-show="isOpen">
		<template v-if="mediaItem">
			<!--	Details Overview Bar	-->
			<v-row class="mx-0">
				<media-overview-bar
					detail-mode
					:library="library"
					:server="server"
					:media-item="mediaItem"
					:has-selected="selected.length > 0"
					@download="downloadSelectedMedia"
					@back="close"
				/>
			</v-row>

			<!--	Header	-->
			<v-row style="max-height: 250px; height: 250px">
				<!--	Poster	-->
				<v-col cols="auto">
					<v-card :max-width="thumbWidth" :width="thumbWidth">
						<v-img :src="imageUrl" :width="thumbWidth" :height="thumbHeight">
							<!--	Placeholder	-->
							<template #placeholder>
								<!--	Show fallback image	-->
								<template v-if="defaultImage">
									<v-row align="center" justify="center" class="fill-height">
										<v-col cols="auto">
											<media-type-icon :size="100" class="mx-3" media-type="mediaType" />
										</v-col>
										<v-col cols="12">
											<h4 class="text-center">{{ mediaItem.title }}</h4>
										</v-col>
									</v-row>
								</template>
								<!--	Show  image	-->
								<template v-else>
									<v-row class="fill-height ma-0" align="center" justify="center">
										<v-col cols="12">
											<h4 class="text-center">{{ mediaItem.title }}</h4>
										</v-col>
										<v-col cols="auto">
											<v-progress-circular indeterminate color="grey lighten-5" />
										</v-col>
									</v-row>
								</template>
							</template>
						</v-img>
					</v-card>
				</v-col>

				<v-col>
					<v-card>
						<v-card-title>
							<h1>{{ mediaItem.title }}</h1>
						</v-card-title>
						<v-card-text>
							<v-row no-gutters>
								<v-col cols="12">
									<span>Duration: <duration :value="mediaItem.duration" /> </span>
								</v-col>
								<v-col cols="12">
									<span>Database ID: {{ mediaItem.id }}</span>
								</v-col>
							</v-row>
						</v-card-text>
					</v-card>
				</v-col>
			</v-row>

			<!--	Media Table	-->
			<v-row no-gutters>
				<v-col>
					<media-table
						ref="detail-media-table"
						:items="[mediaItem]"
						:media-type="mediaType"
						:library-id="library.id"
						hide-navigation
						detail-mode
						@download="downloadMedia"
						@selected="selected = $event"
					/>
				</v-col>
			</v-row>
		</template>
		<!--	Loading	-->
		<template v-else>
			<v-row justify="center">
				<v-col cols="auto">
					<loading-spinner :size="60" />
				</v-col>
			</v-row>
		</template>
	</page>
</template>

<script lang="ts">
import { Component, Prop, Ref, Vue, Watch } from 'vue-property-decorator';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import { DownloadMediaDTO, PlexLibraryDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import MediaOverviewBar from '@mediaOverview/MediaOverviewBar.vue';
import mediaService from '@state/mediaService';

@Component<DetailsOverview>({
	components: {
		MediaTable,
		LoadingSpinner,
		MediaOverviewBar,
	},
})
export default class DetailsOverview extends Vue {
	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ type: Object as () => ITreeViewItem })
	readonly mediaItem!: ITreeViewItem | null;

	@Prop({ required: false, type: Object as () => PlexLibraryDTO | null })
	readonly library!: PlexLibraryDTO | null;

	@Prop({ required: false, type: Object as () => PlexServerDTO | null })
	readonly server!: PlexServerDTO | null;

	@Prop({ required: true, type: Number })
	readonly activeAccountId!: number;

	@Ref('detail-media-table')
	readonly detailMediaTable!: MediaTable;

	private thumbWidth: number = 150;
	private thumbHeight: number = 200;
	isOpen: boolean = false;
	mediaId: number = 0;
	defaultImage: boolean = false;
	imageUrl: string = '';
	selected: string[] = [];
	downloadMediaCommand: DownloadMediaDTO[] = [];
	close(): void {
		this.isOpen = false;
		this.$emit('close');
	}

	@Watch('mediaItem')
	mediaItemIsDone(newMediaItem: ITreeViewItem | null): void {
		if (newMediaItem) {
			mediaService.getThumbnail(newMediaItem.id, this.mediaType, this.thumbWidth, this.thumbHeight).subscribe((imageUrl) => {
				if (!imageUrl) {
					this.defaultImage = true;
					return;
				}
				this.imageUrl = imageUrl;
			});
		}
	}

	openDetails(): void {
		this.isOpen = true;
	}

	downloadSelectedMedia(): void {
		if (this.mediaType === PlexMediaType.TvShow) {
			this.$emit('download', this.detailMediaTable.createDownloadCommands());
		}
		if (this.mediaType === PlexMediaType.Movie) {
			this.$emit('download', {
				mediaIds: [this.mediaItem?.id],
				plexAccountId: this.activeAccountId,
				type: PlexMediaType.Movie,
				libraryId: this.library?.id,
			} as DownloadMediaDTO);
		}
	}

	downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): void {
		this.$emit('download', downloadMediaCommand);
	}
}
</script>
