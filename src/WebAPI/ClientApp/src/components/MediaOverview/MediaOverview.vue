<template>
	<page>
		<template v-if="isLoading">
			<v-row justify="center">
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
			<v-row>
				<media-overview-bar
					:server="server"
					:library="library"
					:view-mode="viewMode"
					@view-change="changeView"
					@refresh-library="refreshLibrary"
				></media-overview-bar>
			</v-row>
			<!--	Data table display	-->
			<template v-if="isTableView">
				<!-- The movie table -->
				<movie-table v-if="isMovieLibrary" :movies="movies" :items="getItems" @download="openDownloadDialog" />
				<!-- The tv-show table -->
				<tv-show-table v-if="isTvShowLibrary" :tv-shows="tvShows" :items="getItems" @download="openDownloadDialog" />
			</template>

			<!-- Poster display-->
			<perfect-scrollbar>
				<v-row v-if="isPosterView" class="poster-overview" justify="center">
					<template v-for="item in getItems">
						<media-poster :key="item.id" :media-item="item" :media-type="mediaType" @download="openDownloadDialog" />
					</template>
				</v-row>
			</perfect-scrollbar>

			<!--	Download confirmation dialog	-->
			<v-row>
				<download-confirmation
					ref="downloadConfirmationRef"
					:items="getItems"
					:progress="downloadTaskCreationProgress"
					@download="downloadMedia"
				/>
			</v-row>
		</template>
		<template v-else>
			<h1>Could not display this library.</h1>
		</template>
	</page>
</template>

<script lang="ts">
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import type { PlexServerDTO } from '@dto/mainApi';
import {
	DownloadTaskCreationProgress,
	LibraryProgress,
	PlexLibraryDTO,
	PlexMediaType,
	PlexMovieDTO,
	PlexTvShowDTO,
	ViewMode,
} from '@dto/mainApi';
import MovieTable from '@mediaOverview/MediaTable/MovieTable.vue';
import MediaPoster from '@mediaOverview/MediaPoster.vue';
import TvShowTable from '@mediaOverview/MediaTable/TvShowTable.vue';
import { merge, of } from 'rxjs';
import SignalrService from '@service/signalrService';
import { catchError, finalize, takeWhile, tap } from 'rxjs/operators';
import Log from 'consola';
import { downloadMedia } from '@api/plexDownloadApi';
import DownloadService from '@state/downloadService';
import ServerService from '@state/serverService';
import ProgressComponent from '@components/ProgressComponent.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import DownloadConfirmation from '@mediaOverview/MediaTable/DownloadConfirmation.vue';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import IMediaId from '@mediaOverview/MediaTable/types/IMediaId';
import MediaOverviewBar from '@mediaOverview/MediaOverviewBar.vue';
import { settingsStore } from '~/store';

@Component({
	components: {
		MediaPoster,
		MovieTable,
		TvShowTable,
		ProgressComponent,
		DownloadConfirmation,
		MediaOverviewBar,
	},
})
export default class MediaOverview extends Vue {
	@Prop({ required: true, type: Number })
	readonly libraryId!: number;

	@Ref('downloadConfirmationRef')
	readonly downloadConfirmationRef!: DownloadConfirmation;

	isLoading: boolean = true;
	isRefreshing: boolean = false;
	server: PlexServerDTO | null = null;
	library: PlexLibraryDTO | null = null;
	libraryProgress: LibraryProgress | null = null;
	downloadTaskCreationProgress: DownloadTaskCreationProgress | null = null;
	downloadPreviewType: PlexMediaType = PlexMediaType.None;

	get mediaType(): PlexMediaType {
		return this.library?.type ?? PlexMediaType.Unknown;
	}

	get activeAccountId(): number {
		return settingsStore.activeAccountId;
	}

	get mediaIds(): number[] {
		return this.getItems.map((x) => x.id);
	}

	get isMovieLibrary(): boolean {
		return this.mediaType === PlexMediaType.Movie;
	}

	get isTvShowLibrary(): boolean {
		return this.mediaType === PlexMediaType.TvShow;
	}

	get movies(): PlexMovieDTO[] {
		return this.library?.movies ?? [];
	}

	get tvShows(): PlexTvShowDTO[] {
		return this.library?.tvShows ?? [];
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
		Log.error('Could not set viewmode for type' + this.mediaType);
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

	get getItems(): ITreeViewItem[] {
		let items: ITreeViewItem[] = [];
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				items = Convert.moviesToTreeViewItems(this.movies);
				break;
			case PlexMediaType.TvShow:
				items = Convert.tvShowsToTreeViewItems(this.tvShows);
				break;
		}

		return items;
	}

	get isPosterView(): boolean {
		return this.viewMode === ViewMode.Poster;
	}

	get isTableView(): boolean {
		return this.viewMode === ViewMode.Table;
	}

	openDownloadDialog(mediaId: IMediaId): void {
		this.downloadConfirmationRef.openDialog(mediaId);
	}

	downloadMedia(mediaId: IMediaId): void {
		merge(
			// Setup progress bar
			SignalrService.getDownloadTaskCreationProgress().pipe(
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
				takeWhile((data) => !data.isComplete),
				catchError(() => {
					return of(null);
				}),
			),
			// Download Media
			downloadMedia(mediaId.id, this.activeAccountId, mediaId.type).pipe(
				finalize(() => {
					setTimeout(() => {
						this.downloadConfirmationRef.closeDialog();
						this.downloadTaskCreationProgress = null;
					}, 2000);
					DownloadService.fetchDownloadList();
				}),
				catchError(() => {
					return of(false);
				}),
			),
		)
			.pipe(
				catchError(() => {
					this.downloadConfirmationRef.closeDialog();
					this.downloadTaskCreationProgress = null;
					return of(false);
				}),
			)
			.subscribe();
	}

	refreshLibrary(): void {
		this.isRefreshing = true;
		this.isLoading = true;
		this.resetProgress(true);
		ServerService.refreshLibrary(this.libraryId);
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

		ServerService.getServerByLibraryID(this.libraryId).subscribe((server) => {
			this.server = server ?? null;
			Log.warn('Server:', this.server);
		});

		ServerService.getLibrary(this.libraryId).subscribe((library) => {
			if (library) {
				this.library = library;
				Log.warn('Library:', this.library);
				if (this.library.count > -1) {
					this.isLoading = false;
				}
			}
		});
	}
}
</script>
