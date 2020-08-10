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
		<!-- The movie table -->
		<v-row v-if="activeAccount">
			<v-col>
				<tv-show-table :tvshows="tvshows" :active-account="activeAccount" :selected.sync="selected" :loading="isLoading" />
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
import { ITvShowSelector, ISeasonSelector, IEpisodeSelector } from './types/iTvShowSelector';

import TvShowTable from './components/TvShowTable.vue';

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

	selected: ITvShowSelector[] = [];

	get tvshows(): PlexTvShowDTO[] {
		return this.library?.tvShows ?? [];
	}

	get server(): PlexServerDTO | null {
		return this.activeAccount?.plexServers.find((x) => x.id === this.library?.plexServerId) ?? null;
	}

	refreshLibrary(): void {
		this.isLoading = true;

		refreshPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).subscribe((data) => {
			this.library = data;
			this.isLoading = false;
		});
	}

	setSelectionTree(): void {
		// Create a selected structure based on every tvShow, season and episode
		this.tvshows.forEach((tvShow) => {
			const seasons: ISeasonSelector[] = [];
			if (tvShow.seasons) {
				tvShow.seasons.forEach((season) => {
					const episodes: IEpisodeSelector[] = [];
					if (season.episodes) {
						season.episodes.forEach((episode) => {
							// Add episodes
							episodes.push({ id: episode.id, selected: false });
						});
					}
					// Add seasons
					seasons.push({ id: season.id, selected: false, episodes });
				});
			}
			// Add tvShows
			this.selected.push({ id: tvShow.id, selected: true, seasons });
		});
		Log.warn(this.selected);
	}

	getLibrary(): void {
		this.isLoading = true;
		getPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).subscribe((data) => {
			Log.debug(`TvShowsDetail => Library: ${data?.id}`, data);
			this.library = data;
			this.setSelectionTree();
			this.isLoading = false;
		});
	}

	created(): void {
		this.libraryId = +this.$route.params.id;
		SettingsService.getActiveAccount().subscribe((data) => {
			Log.debug(`TvShowsDetail => ${data?.id}`, data);
			this.activeAccount = data ?? null;
			this.getLibrary();
		});
	}
}
</script>
