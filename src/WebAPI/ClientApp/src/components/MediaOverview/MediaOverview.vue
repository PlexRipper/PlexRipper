<template>
	<page no-scrollbar>
		<!--	Loading screen	-->
		<template v-if="isLoading">
			<v-row justify="center" class="mx-0">
				<v-col cols="auto">
					<v-layout row justify-center align-center>
						<v-progress-circular :size="70" :width="7" color="red" indeterminate></v-progress-circular>
					</v-layout>
					<h1 v-if="isRefreshing">
						Refreshing library "{{ library ? library.title : '' }}" data {{ server ? 'from ' + server.name : '' }}
					</h1>
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
			<v-row v-show="showMediaOverview" no-gutters>
				<v-col>
					<!--	Overview bar	-->
					<v-row>
						<media-overview-bar
							:server="server"
							:library="library"
							:view-mode="viewMode"
							:has-selected="selected.length > 0"
							:hide-download-button="!isTableView"
							@view-change="changeView"
							@refresh-library="refreshLibrary"
							@download="processDownloadCommand([])"
						></media-overview-bar>
					</v-row>
					<!--	Data table display	-->
					<template v-if="isTableView">
						<media-table
							ref="overviewMediaTable"
							:items="items"
							:active-account-id="activeAccountId"
							:library-id="libraryId"
							:media-type="mediaType"
							@download="processDownloadCommand"
							@selected="selected = $event"
							@request-media="requestMedia"
						/>
					</template>

					<!-- Poster display-->
					<template v-if="isPosterView">
						<poster-table
							:items="items"
							:active-account-id="activeAccountId"
							:media-type="mediaType"
							@download="processDownloadCommand"
							@open-details="openDetails"
						/>
					</template>
				</v-col>
			</v-row>
			<!--	Overlay with details of the media	-->
			<details-overview
				v-show="!showMediaOverview"
				ref="detailsOverview"
				:media-type="mediaType"
				:media-item="detailItem"
				:library="library"
				:server="server"
				:active-account-id="activeAccountId"
				@close="closeDetailsOverview"
				@download="processDownloadCommand"
			/>
		</template>
		<template v-else>
			<h1>Could not display this library.</h1>
		</template>
		<!--	Download confirmation dialog	-->
		<download-confirmation
			ref="downloadConfirmationRef"
			:items="items"
			:progress="downloadTaskCreationProgress"
			@download="sendDownloadCommand"
		/>
	</page>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Ref, Vue, Watch } from 'vue-property-decorator';
import { finalize, map, tap } from 'rxjs/operators';
import type { DownloadMediaDTO, PlexMediaDTO, PlexServerDTO } from '@dto/mainApi';
import { DownloadTaskCreationProgress, LibraryProgress, PlexLibraryDTO, PlexMediaType, ViewMode } from '@dto/mainApi';
import { DownloadService, LibraryService, SettingsService, SignalrService } from '@service';
import { MediaTable, MediaOverviewBar, MediaPoster, PosterTable, DetailsOverview, DownloadConfirmation } from '@mediaOverview';
import ProgressComponent from '@components/Progress/ProgressComponent.vue';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';
import { getTvShow } from '@api/mediaApi';

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

	@Ref('overviewMediaTable')
	readonly overviewMediaTableRef!: MediaTable;

	activeAccountId: number = 0;
	movieViewMode: ViewMode = ViewMode.Poster;
	tvShowViewMode: ViewMode = ViewMode.Poster;
	selected: string[] = [];
	isRefreshing: boolean = false;
	server: PlexServerDTO | null = null;
	library: PlexLibraryDTO | null = null;
	libraryProgress: LibraryProgress | null = null;
	downloadTaskCreationProgress: DownloadTaskCreationProgress | null = null;
	downloadPreviewType: PlexMediaType = PlexMediaType.None;
	items: PlexMediaDTO[] = [];
	detailItem: PlexMediaDTO | null = null;

	get mediaType(): PlexMediaType {
		return this.library?.type ?? PlexMediaType.Unknown;
	}

	get getPercentage(): number {
		return this.libraryProgress?.percentage ?? -1;
	}

	get isLoading(): boolean {
		return this.isRefreshing || !(this.server && this.library);
	}

	get viewMode(): ViewMode {
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				return this.movieViewMode;
			case PlexMediaType.TvShow:
				return this.tvShowViewMode;
			default:
				return ViewMode.Poster;
		}
	}

	get showMediaOverview(): boolean {
		return !(this.detailItem ?? false);
	}

	changeView(viewMode: ViewMode): void {
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				return SettingsService.updateMovieViewMode(viewMode);
			case PlexMediaType.TvShow:
				return SettingsService.updateTvShowViewMode(viewMode);
		}
		Log.error('Could not set view mode for type' + this.mediaType);
	}

	resetProgress(isRefreshing: boolean): void {
		this.isRefreshing = isRefreshing;
		this.libraryProgress = {
			id: this.libraryId,
			percentage: 0,
			received: 0,
			total: 0,
			isRefreshing,
			isComplete: false,
			timeStamp: '',
		};
	}

	get isPosterView(): boolean {
		return this.viewMode === ViewMode.Poster;
	}

	get isTableView(): boolean {
		return this.viewMode === ViewMode.Table;
	}

	processDownloadCommand(downloadMediaCommand: DownloadMediaDTO[]): void {
		if (downloadMediaCommand.length > 0) {
			this.downloadConfirmationRef.openDialog(downloadMediaCommand);
			return;
		}
		if (this.overviewMediaTableRef) {
			this.downloadConfirmationRef.openDialog(this.overviewMediaTableRef.createDownloadCommands());
		} else {
			Log.error('overviewMediaTableRef was invalid', this.overviewMediaTableRef);
		}
	}

	sendDownloadCommand(downloadMediaCommand: DownloadMediaDTO[]): void {
		DownloadService.downloadMedia(downloadMediaCommand);
	}

	@Watch('$route.path')
	testFunction(newPath: string, oldPath: string): void {
		if (oldPath.includes('details') && !newPath.includes('details')) {
			this.resetDetailsOverview();
		}
	}

	openDetails(mediaId: number): void {
		if (!this.$route.path.includes('details')) {
			this.$router.push({
				path: this.libraryId + '/details/' + mediaId,
			});
		}
		this.detailsOverview?.openDetails();

		const item = this.items.find((x) => x.id === mediaId);
		if (item?.children?.length === 0) {
			this.requestMedia({
				item,
				resolve: () => {
					this.detailItem = this.items.find((x) => x.id === mediaId) ?? null;
				},
			});
		} else {
			this.detailItem = item ?? null;
		}
	}

	closeDetailsOverview(): void {
		Log.debug('Close Details Overview');
		this.$router.push({
			path: '/tvshows/' + this.libraryId,
		});
		this.resetDetailsOverview();
	}

	resetDetailsOverview(): void {
		this.detailItem = null;
	}

	refreshLibrary(): void {
		this.isRefreshing = true;
		this.resetProgress(true);
		LibraryService.refreshLibrary(this.libraryId).subscribe((data) => {
			this.setLibrary(data);
			this.isRefreshing = false;
		});
	}

	setLibrary(data: PlexLibraryDTO | null): void {
		if (data) {
			this.library = data;
			switch (this.mediaType) {
				case PlexMediaType.Movie:
					this.items = this.library?.movies ?? [];
					break;
				case PlexMediaType.TvShow:
					this.items = this.library?.tvShows ?? [];
					break;
			}
		}
	}

	requestMedia(numberPromise: { item: PlexMediaDTO; resolve?: Function }): void {
		if (this.mediaType === PlexMediaType.TvShow) {
			getTvShow(numberPromise.item.id).subscribe((response) => {
				if (response.isSuccess) {
					const itemsIndex = this.items.findIndex((x) => x.id === numberPromise.item.id);
					// This is a fix to prevent episodes from acting like it has additional children and that it can be requested
					response.value?.children?.forEach((season) => {
						season.children?.forEach((episode) => {
							// @ts-ignore:
							episode.children = undefined;
						});
					});
					this.items[itemsIndex].children?.push(...(response.value?.children ?? []));
				}
				if (numberPromise.resolve) {
					// Alert listener that the data is available
					numberPromise.resolve(response?.value);
				}
			});
		} else {
			Log.error('Request media could not be executed for ' + this.mediaType);
		}
	}

	created(): void {
		this.resetProgress(false);
		this.isRefreshing = false;

		// Get Active account Id
		this.$subscribeTo(SettingsService.getActiveAccountId(), (id) => (this.activeAccountId = id));

		// Get display settings
		this.$subscribeTo(SettingsService.getDisplaySettings(), (x) => {
			this.movieViewMode = x.movieViewMode;
			this.tvShowViewMode = x.tvShowViewMode;
		});

		// Setup progress bar
		this.$subscribeTo(SignalrService.getLibraryProgress(this.libraryId), (data) => {
			if (data) {
				this.libraryProgress = data;
				this.isRefreshing = data.isRefreshing;
				if (data.isComplete) {
					Log.debug(data);
					this.resetProgress(false);
					LibraryService.fetchLibrary(this.libraryId);
				}
			}
		});

		// Show DownloadTask creation progress window
		this.$subscribeTo(
			SignalrService.getDownloadTaskCreationProgress().pipe(
				tap((data) => {
					this.downloadTaskCreationProgress = data;
				}),
				finalize(() => {
					setTimeout(() => {
						this.downloadConfirmationRef?.closeDialog();
						this.downloadTaskCreationProgress = null;
					}, 2000);
				}),
			),
			() => {},
		);

		// Retrieve server data
		this.$subscribeTo(LibraryService.getServerByLibraryID(this.libraryId), (server) => {
			if (server) {
				this.server = server;
				return;
			}
			Log.warn('MediaOverview => Server was invalid:', server);
		});

		// Retrieve library data
		this.$subscribeTo(LibraryService.getLibrary(this.libraryId), (data) => this.setLibrary(data));
	}

	mounted(): void {
		this.$subscribeTo(
			this.$watchAsObservable('isLoading').pipe(map((x: { oldValue: number; newValue: number }) => x.newValue)),
			(isLoading) => {
				if (!isLoading) {
					if (this.detailsOverview) {
						if (+this.$route.params.mediaid) {
							this.openDetails(+this.$route.params.mediaid);
						}
					} else {
						const thisRef = this;
						this.$nextTick(() => {
							Log.debug('mediaId', +this.$route.params.mediaid);
							if (+this.$route.params.mediaid) {
								thisRef?.openDetails(+this.$route.params.mediaid);
							}
						});
					}
				}
			},
		);
	}
}
</script>
