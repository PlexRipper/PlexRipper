<template>
	<v-container>
		<v-row justify="space-between">
			<v-col cols="6">
				<h1>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</h1>
			</v-col>
			<v-col cols="auto">
				<v-btn :disabled="getIsRefreshing" @click="refreshLibrary">Refresh Library</v-btn>
			</v-col>
		</v-row>
		<!-- The tv show table -->
		<v-row v-if="activeAccount" justify="center">
			<v-col v-if="isLoading" cols="auto">
				<v-layout row justify-center align-center>
					<v-progress-circular :size="70" :width="7" color="red" indeterminate></v-progress-circular>
				</v-layout>
				<h1 v-if="getIsRefreshing">Refreshing library data from {{ server ? server.name : 'unknown' }}</h1>
				<h1 v-else>Retrieving library from PlexRipper database</h1>
				<!-- Library progress bar -->
				<v-progress-linear :value="getPercentage" height="20" striped color="deep-orange">
					<template v-slot="{ value }">
						<strong>{{ value }}%</strong>
					</template>
				</v-progress-linear>
			</v-col>
			<v-col v-else>
				<tv-show-table :tvshows="tvshows" :active-account="activeAccount" :selected="selected" :loading="isLoading" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';
import SettingsService from '@service/settingsService';
import { PlexAccountDTO, PlexTvShowDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import SignalrService from '@service/signalrService';
import { takeWhile, tap, switchMap } from 'rxjs/operators';
import { Subscription, merge } from 'rxjs';
import { ITvShowSelector } from './types/iTvShowSelector';

import TvShowTable from './components/TvShowTable.vue';
import ILibraryProgress from '~/types/dto/ILibraryProgress';

@Component<TvShowsDetail>({
	components: {
		LoadingSpinner,
		TvShowTable,
	},
})
export default class TvShowsDetail extends Vue {
	activeAccount: PlexAccountDTO | null = null;

	libraryId: number = 0;

	library: PlexLibraryDTO | null = null;

	isLoading: boolean = true;

	isRefreshing: boolean = false;

	selected: ITvShowSelector[] = [];

	progress: ILibraryProgress | null = null;

	libraryProgressSubscription: Subscription | null = null;

	get tvshows(): PlexTvShowDTO[] {
		return this.library?.tvShows ?? [];
	}

	get server(): PlexServerDTO | null {
		return this.activeAccount?.plexServers.find((x) => x.id === this.library?.plexServerId) ?? null;
	}

	get getPercentage(): number {
		return this.progress?.percentage ?? 0;
	}

	get getIsRefreshing(): boolean {
		return this.progress?.isRefreshing ?? this.isRefreshing;
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
			// Refresh Library
			refreshPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).pipe(
				tap((data) => {
					Log.debug(`TvShowsDetail => refreshPlexLibrary: ${data?.id}`, data);
					this.library = data;
					this.isLoading = false;
					this.isRefreshing = false;
				}),
			),
			// Setup progress bar
			SignalrService.getLibraryProgress().pipe(
				tap((data) => {
					this.progress = data;
					this.isRefreshing = data.isRefreshing;
					Log.debug(data);
				}),
				takeWhile((data) => !data.isComplete),
			),
		).subscribe();
	}

	setSelected(selected: ITvShowSelector[]): void {
		Log.debug(selected);
		this.selected = selected;
	}

	created(): void {
		this.libraryId = +this.$route.params.id;
		this.resetProgress(false);
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
								this.isRefreshing = data.isRefreshing;
								Log.debug(data);
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
						),
					),
				),
			)
			.subscribe();
	}

	destroyed() {
		if (this.libraryProgressSubscription) {
			this.libraryProgressSubscription.unsubscribe();
		}
	}
}
</script>
