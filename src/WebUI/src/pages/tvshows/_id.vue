<template>
	<v-container v-if="activeAccount">
		<!-- The tv show table -->
		<v-row v-if="isLoading" justify="center">
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
		<v-row v-else justify="space-between">
			<v-col cols="6">
				<h1>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</h1>
			</v-col>
			<v-col cols="auto">
				<v-btn :disabled="isRefreshing" @click="refreshLibrary">Refresh Library</v-btn>
			</v-col>
			<v-col cols="12">
				<tv-show-table :tvshows="tvshows" :active-account="activeAccount" :loading="isLoading" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';
import SettingsService from '@service/settingsService';
import { PlexAccountDTO, PlexTvShowDTO, PlexLibraryDTO, PlexServerDTO, LibraryProgress } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import SignalrService from '@service/signalrService';
import { takeWhile, tap, switchMap, takeLast, finalize } from 'rxjs/operators';
import { Subscription, merge } from 'rxjs';

import TvShowTable from './components/TvShowTable.vue';

@Component<TvShowsDetail>({
	components: {
		LoadingSpinner,
		TvShowTable,
	},
})
export default class TvShowsDetail extends Vue {
	activeAccount: PlexAccountDTO | null = null;

	library: PlexLibraryDTO | null = null;

	isLoading: boolean = true;

	isRefreshing: boolean = false;

	progress: LibraryProgress | null = null;

	libraryProgressSubscription: Subscription | null = null;

	get tvshows(): PlexTvShowDTO[] {
		return this.library?.tvShows ?? [];
	}

	get server(): PlexServerDTO | null {
		return (
			this.activeAccount?.plexServers.find((x) => x.plexLibraries?.find((x) => x.id === this.getLibraryId) !== undefined) ?? null
		);
	}

	get getPercentage(): number {
		return this.progress?.percentage ?? 0;
	}

	get getLibraryId(): number {
		return +this.$route?.params?.id ?? 0;
	}

	resetProgress(isRefreshing: boolean): void {
		this.progress = {
			id: this.getLibraryId,
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
			refreshPlexLibrary(this.getLibraryId, this.activeAccount?.id ?? 0).pipe(
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
						getPlexLibrary(this.getLibraryId, data?.id ?? 0).pipe(
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
