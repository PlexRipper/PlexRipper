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
import { PlexAccountDTO, PlexTvShowDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';

import LoadingSpinner from '@/components/LoadingSpinner.vue';
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

	getLibrary(): void {
		this.isLoading = true;
		getPlexLibrary(this.libraryId, this.activeAccount?.id ?? 0).subscribe((data) => {
			Log.debug(`TvShowsDetail => Library: ${data?.id}`, data);
			this.library = data;
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
