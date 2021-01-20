<template>
	<page>
		<template v-if="isLoading">
			<v-row justify="center" class="mx-0">
				<v-col cols="auto">
					<v-layout row justify-center align-center>
						<v-progress-circular :size="70" :width="7" color="red" indeterminate></v-progress-circular>
					</v-layout>
					<h1 v-if="isRefreshing">Refreshing library data from {{ server ? server.name : 'unknown' }}</h1>
					<h1 v-else>Retrieving library from PlexRipper database</h1>
					<!-- Library progress bar -->
					<v-progress-linear :value="getPercentage" height="20" striped color="deep-orange">
						<template #default="{ value }">
							<strong>{{ value }}%</strong>
						</template>
					</v-progress-linear>
				</v-col>
			</v-row>
		</template>
		<!-- Header -->
		<template v-else-if="server && library">
			<!--	Overview bar	-->
			<v-row class="mx-0">
				<media-overview-bar
					:server="server"
					:library="library"
					:view-mode="viewMode"
					:has-selected="getSelectedMediaIds.length > 0"
					@view-change="changeView"
					@refresh-library="refreshLibrary"
					@download="processDownloadCommand(null)"
				></media-overview-bar>
			</v-row>
			<!--	Data table display	-->
			<template v-if="isTableView">
				<media-table
					:items="items"
					:media-type="mediaType"
					@download="processDownloadCommand"
					@selected="selected = $event"
					@request-media="requestMedia"
				/>
			</template>

			<!-- Poster display-->
			<template v-if="isPosterView">
				<poster-table :items="items" :media-type="mediaType" @download="processDownloadCommand" @open-details="openDetails" />
			</template>

			<!--	Download confirmation dialog	-->
			<v-row>
				<download-confirmation
					ref="downloadConfirmationRef"
					:items="items"
					:progress="downloadTaskCreationProgress"
					@download="sendDownloadCommand"
				/>
			</v-row>

			<!--	Overlay with details of the media	-->
			<details-overview ref="detailsOverview" :media-type="mediaType" :media-item="detailItem" @close="closeDetails" />
		</template>
		<template v-else>
			<h1>Could not display this library.</h1>
		</template>
	</page>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import { filter, finalize, tap } from 'rxjs/operators';
import type { DownloadMediaDTO, PlexServerDTO } from '@dto/mainApi';
import { DownloadTaskCreationProgress, LibraryProgress, PlexLibraryDTO, PlexMediaType, ViewMode } from '@dto/mainApi';
import MediaPoster from '@mediaOverview/PosterTable/MediaPoster.vue';
import SignalrService from '@service/signalrService';
import DownloadService from '@state/downloadService';
import LibraryService from '@state/libraryService';
import ProgressComponent from '@components/ProgressComponent.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import DownloadConfirmation from '@mediaOverview/MediaTable/DownloadConfirmation.vue';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import MediaOverviewBar from '@mediaOverview/MediaOverviewBar.vue';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';
import { combineLatest } from 'rxjs';
import PosterTable from '@mediaOverview/PosterTable/PosterTable.vue';
import DetailsOverview from '@mediaOverview/DetailsOverview.vue';
import { getTvShow } from '@api/mediaApi';
import { settingsStore } from '~/store';

@Component({
	components: {
		MediaPoster,
		MediaTable,
		ProgressComponent,
		DownloadConfirmation,
		MediaOverviewBar,
		AlphabetNavigation,
		PosterTable,
		DetailsOverview,
	},
})
export default class MediaOverview extends Vue {
	@Prop({ required: true, type: Number })
	readonly libraryId!: number;

	@Ref('downloadConfirmationRef')
	readonly downloadConfirmationRef!: DownloadConfirmation;

	@Ref('detailsOverview')
	readonly detailsOverview!: DetailsOverview;

	selected: string[] = [];
	isLoading: boolean = true;
	isRefreshing: boolean = false;
	server: PlexServerDTO | null = null;
	library: PlexLibraryDTO | null = null;
	libraryProgress: LibraryProgress | null = null;
	downloadTaskCreationProgress: DownloadTaskCreationProgress | null = null;
	downloadPreviewType: PlexMediaType = PlexMediaType.None;
	items: ITreeViewItem[] = [];
	detailItem: ITreeViewItem | null = null;

	get mediaType(): PlexMediaType {
		return this.library?.type ?? PlexMediaType.Unknown;
	}

	get activeAccountId(): number {
		return settingsStore.activeAccountId;
	}

	get getPercentage(): number {
		return this.libraryProgress?.percentage ?? -1;
	}

	get viewMode(): ViewMode {
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				return settingsStore.movieViewMode;
			case PlexMediaType.TvShow:
				return settingsStore.tvShowViewMode;
			default:
				return ViewMode.Poster;
		}
	}

	changeView(viewMode: ViewMode): void {
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				return settingsStore.setMovieViewMode(viewMode);
			case PlexMediaType.TvShow:
				return settingsStore.setTvShowViewMode(viewMode);
		}
		Log.error('Could not set view mode for type' + this.mediaType);
	}

	resetProgress(isRefreshing: boolean): void {
		this.libraryProgress = {
			id: this.libraryId,
			percentage: 0,
			received: 0,
			total: 0,
			isRefreshing,
			isComplete: false,
		};
	}

	convertMediaItems(): void {
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				this.items = Convert.moviesToTreeViewItems(this.library?.movies ?? []);
				break;
			case PlexMediaType.TvShow:
				this.items = Convert.tvShowsToTreeViewItems(this.library?.tvShows ?? []);
				break;
		}
	}

	get isPosterView(): boolean {
		return this.viewMode === ViewMode.Poster;
	}

	get isTableView(): boolean {
		return this.viewMode === ViewMode.Table;
	}

	openDownloadDialog(downloadMediaCommand: DownloadMediaDTO): void {
		this.downloadConfirmationRef.openDialog(downloadMediaCommand);
	}

	get getSelectedMediaIds(): number[] {
		const ids: number[] = [];
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				this.selected.forEach((x) => ids.push(+x.split('-')[0]));
				break;
			case PlexMediaType.TvShow:
			case PlexMediaType.Season:
			case PlexMediaType.Episode:
				this.selected.forEach((x) => ids.push(+x.split('-')[2]));
				break;
			default:
				Log.warn('Could not determine the type of the media to correctly download multiple selected media');
				break;
		}
		return ids;
	}

	get downloadMediaCommand(): DownloadMediaDTO {
		let type: PlexMediaType = PlexMediaType.None;

		// Determine the type of media downloaded, the getSelectedMediaIds are always of the same type.
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				type = PlexMediaType.Movie;
				break;
			case PlexMediaType.TvShow:
			case PlexMediaType.Season:
			case PlexMediaType.Episode:
				type = PlexMediaType.Episode;
				break;
			default:
				return {} as DownloadMediaDTO;
		}
		return {
			mediaIds: this.getSelectedMediaIds,
			type,
			plexAccountId: this.activeAccountId,
			libraryId: this.libraryId,
		};
	}

	processDownloadCommand(downloadMediaCommand: DownloadMediaDTO | null): void {
		if (downloadMediaCommand) {
			downloadMediaCommand.libraryId = this.libraryId;
			downloadMediaCommand.plexAccountId = this.activeAccountId;

			this.openDownloadDialog(downloadMediaCommand);
		} else {
			this.openDownloadDialog(this.downloadMediaCommand);
		}
	}

	sendDownloadCommand(downloadMediaCommand: DownloadMediaDTO): void {
		DownloadService.downloadMedia(downloadMediaCommand);
	}

	openDetails(mediaId: number): void {
		this.detailsOverview.openDetails();
		const item = this.items.find((x) => x.id === mediaId);
		if (item?.children?.length === 0) {
			this.requestMedia({
				mediaId,
				resolve: () => {
					this.detailItem = this.items.find((x) => x.id === mediaId) ?? null;
				},
			});
		} else {
			this.detailItem = item ?? null;
		}
	}

	closeDetails(): void {
		this.detailItem = null;
	}

	refreshLibrary(): void {
		this.isRefreshing = true;
		this.isLoading = true;
		this.library = null;
		this.resetProgress(true);
		LibraryService.refreshLibrary(this.libraryId);
	}

	requestMedia(numberPromise: { mediaId: number; resolve?: Function }): void {
		if (this.mediaType === PlexMediaType.TvShow) {
			getTvShow(numberPromise.mediaId).subscribe((response) => {
				if (response) {
					const convert = Convert.tvShowsToTreeViewItems([response])[0];
					const itemsIndex = this.items.findIndex((x) => x.id === numberPromise.mediaId);
					this.items[itemsIndex].children?.push(...(convert?.children ?? []));
				}
				if (numberPromise.resolve) {
					// Alert listener that the data is available
					numberPromise.resolve();
				}
			});
		} else {
			Log.error('Request media could not be executed for ' + this.mediaType);
		}
	}

	created(): void {
		this.resetProgress(false);
		this.isRefreshing = false;
		this.isLoading = true;

		// Setup progress bar
		SignalrService.getLibraryProgress().subscribe((data) => {
			if (data.id === this.libraryId) {
				this.libraryProgress = data;
				this.isRefreshing = data.isRefreshing ?? false;
			}
		});

		SignalrService.getDownloadTaskCreationProgress()
			.pipe(
				filter((x) => x.plexLibraryId === this.libraryId),
				tap((data) => {
					this.downloadTaskCreationProgress = data;
					Log.debug(data);
				}),
				finalize(() => {
					setTimeout(() => {
						this.downloadConfirmationRef.closeDialog();
						this.downloadTaskCreationProgress = null;
					}, 2000);
				}),
			)
			.subscribe();

		// Retrieve server and library data
		combineLatest([LibraryService.getServerByLibraryID(this.libraryId), LibraryService.getLibrary(this.libraryId)]).subscribe(
			(data) => {
				if (data[0]) {
					this.server = Object.freeze(data[0]);
				}
				if (data[1] && data[1].id === this.libraryId) {
					this.library = Object.freeze(data[1]);
					this.convertMediaItems();
				}
				if (this.server && this.library) {
					this.isLoading = false;
				}
			},
		);
	}
}
</script>
