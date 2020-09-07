<template>
	<v-container>
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
						<template v-slot="{ value }">
							<strong>{{ value }}%</strong>
						</template>
					</v-progress-linear>
				</v-col>
			</v-row>
		</template>
		<!-- Header -->
		<template v-else>
			<v-row justify="space-between">
				<v-col cols="6">
					<h1>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</h1>
				</v-col>
				<v-col cols="auto">
					<v-btn @click="refreshLibrary">Refresh Library</v-btn>
				</v-col>
			</v-row>
			<!--	Overview bar	-->
			<v-row>
				<v-col cols="12">
					<v-switch v-model="imageView" label="Show images" />
				</v-col>
			</v-row>
			<!--	Data table display	-->
			<template v-if="!imageView">
				<!-- The movie table -->
				<movie-table v-if="isMovieLibrary" :movies="movies" :account-id="activeAccountId" />
				<!-- The tv-show table -->
				<tv-show-table v-if="isTvShowLibrary" :tv-shows="tvShows" :active-account="activeAccount" />
			</template>

			<!-- Poster display-->
			<v-row v-else>
				<template v-for="mediaId in mediaIds">
					<media-poster :key="mediaId" :media-id="mediaId" :account-id="activeAccountId" :media-type="mediaType"></media-poster>
				</template>
			</v-row>
		</template>
	</v-container>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { LibraryProgress, PlexLibraryDTO, PlexMediaType, PlexMovieDTO, PlexServerDTO, PlexTvShowDTO } from '@dto/mainApi';
import type { PlexAccountDTO } from '@dto/mainApi';
import MovieTable from '@mediaOverview/MediaTable/MovieTable.vue';
import MediaPoster from '@mediaOverview/MediaPoster.vue';
import TvShowTable from '@mediaOverview/MediaTable/TvShowTable.vue';
import { merge } from 'rxjs';
import SignalrService from '@service/signalrService';
import { finalize, switchMap, takeLast, takeWhile, tap } from 'rxjs/operators';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';
import Log from 'consola';
import SettingsService from '@service/settingsService';

@Component<MediaOverview>({
	components: {
		MediaPoster,
		MovieTable,
		TvShowTable,
	},
})
export default class MediaOverview extends Vue {
	@Prop({ required: true, type: Number })
	readonly libraryId!: number;

	activeAccount: PlexAccountDTO | null = null;

	isLoading: boolean = true;
	imageView: boolean = true;
	isRefreshing: boolean = false;
	progress: LibraryProgress | null = null;
	library: PlexLibraryDTO | null = null;

	get activeAccountId(): number {
		return this.activeAccount?.id ?? 0;
	}

	get mediaType(): PlexMediaType {
		return this.library?.type ?? PlexMediaType.Unknown;
	}

	get mediaIds(): number[] {
		switch (this.mediaType) {
			case PlexMediaType.Movie:
				return this.movies.map((x) => x.id);
			case PlexMediaType.TvShow:
				// Return the id's of the epi
				return this.tvShows.map((x) => x.id);
			default:
				return [];
		}
	}

	get server(): PlexServerDTO | null {
		return this.activeAccount?.plexServers.find((x) => x.id === this.library?.plexServerId) ?? null;
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
		return this.progress?.percentage ?? 0;
	}

	resetProgress(isRefreshing: boolean): void {
		this.progress = {
			id: this.libraryId,
			percentage: 0,
			received: 0,
			total: 0,
			isRefreshing,
			isComplete: false,
		};
	}

	refreshLibrary(): void {
		this.isRefreshing = true;
		this.isLoading = true;
		this.resetProgress(true);
		merge(
			// Setup progress bar
			SignalrService.getLibraryProgress().pipe(
				tap((data) => {
					this.progress = data;
				}),
				finalize(() => {
					this.progress = null;
				}),
				takeWhile((data) => !data.isComplete),
			),
			// Refresh Library
			refreshPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).pipe(
				tap((data) => {
					Log.debug(`TvShowsDetail => refreshPlexLibrary: ${data?.id}`, data);
					this.library = data;
					this.isLoading = false;
					this.isRefreshing = false;
				}),
				takeLast(1),
			),
		).subscribe();
	}

	created(): void {
		this.resetProgress(false);
		this.isRefreshing = false;
		this.isLoading = true;

		SettingsService.getActiveAccount()
			.pipe(
				tap((data) => {
					this.activeAccount = data ?? null;
				}),
				switchMap((data) =>
					merge(
						// Setup progress bar
						SignalrService.getLibraryProgress().pipe(
							tap((data) => {
								this.progress = data;
								this.isRefreshing = data.isRefreshing ?? false;
							}),
							takeWhile((data) => !data.isComplete),
						),
						// Retrieve library
						getPlexLibrary(this.libraryId, data?.id ?? 0).pipe(
							tap((data) => {
								this.library = data;
								Log.debug(`TvShowsDetail => Library: ${data?.id}`, data);
								this.isLoading = false;
							}),
							takeLast(1),
						),
					),
				),
			)
			.subscribe();
	}
}
</script>
