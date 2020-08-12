<template>
	<v-container>
		<v-row justify="space-between">
			<v-col cols="6">
				<h1>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</h1>
			</v-col>
			<v-col cols="auto">
				<v-btn @click="refreshLibrary">Refresh Library</v-btn>
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
import { finalize, takeWhile, tap } from 'rxjs/operators';
import { Subscription } from 'rxjs';
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

	refreshLibrary(): void {
		this.isRefreshing = true;
		this.isLoading = true;

		refreshPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0)
			.pipe(
				finalize(() => {
					this.isRefreshing = false;
				}),
			)
			.subscribe((data) => {
				this.library = data;
				this.isLoading = false;
			});
	}

	setSelected(selected: ITvShowSelector[]): void {
		Log.debug(selected);
		this.selected = selected;
	}

	getLibrary(): void {
		this.isLoading = true;

		this.libraryProgressSubscription = SignalrService.getLibraryProgress()
			.pipe(
				tap((data) => Log.debug(data)),
				takeWhile((data) => !data.isComplete),
				finalize(() =>
					getPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).subscribe((data) => {
						this.library = data;
						Log.debug(`TvShowsDetail => Library: ${data?.id}`, data);
						this.isLoading = false;
					}),
				),
			)
			.subscribe((data) => {
				Log.debug(data);
				this.progress = data;
				this.isRefreshing = data.isRefreshing;
			});

		getPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).subscribe((data) => {
			this.library = data;
			Log.debug(`TvShowsDetail => Library: ${data?.id}`, data);
			this.isLoading = false;
		});
	}

	created(): void {
		this.libraryId = +this.$route.params.id;

		// const x = concat(
		// 	SignalrService.getLibraryProgress().pipe(
		// 		tap((data) => Log.debug(data)),
		// 		takeWhile((data) => !data.isComplete),
		// 	),
		// 	getPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0),
		// ).subscribe((data) => {
		// 	Log.debug('Test:', data);
		// });

		SettingsService.getActiveAccount().subscribe((data) => {
			this.activeAccount = data ?? null;
			this.getLibrary();
		});
	}

	destroyed() {
		if (this.libraryProgressSubscription) {
			this.libraryProgressSubscription.unsubscribe();
		}
	}
}
</script>
