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
		<v-row v-if="activeAccount">
			<v-col cols="12"> <v-switch v-model="imageView" label="Show images"></v-switch></v-col>
			<!-- The movie table -->
			<v-col v-if="!imageView" cols="12">
				<movie-table :movies="movies" :active-account="activeAccount" :loading="isLoading" />
			</v-col>
			<v-col v-else cols="12">
				<v-row>
					<template v-for="(movie, i) in movies">
						<media-poster :key="i" :movie="movie" :account-id="activeAccountId" :media-type="libraryType"></media-poster>
					</template>
				</v-row>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';
import SettingsService from '@service/settingsService';
import { PlexAccountDTO, PlexLibraryDTO, PlexMediaType, PlexMovieDTO, PlexServerDTO } from '@dto/mainApi';

import MovieTable from './components/MovieTable.vue';
import MediaPoster from './components/MediaPoster.vue';

@Component<MoviesDetail>({
	components: {
		MediaPoster,
		MovieTable,
	},
})
export default class MoviesDetail extends Vue {
	activeAccount: PlexAccountDTO | null = null;

	libraryId: number = 0;

	library: PlexLibraryDTO | null = null;

	isLoading: boolean = true;

	imageUrl: string = '';

	imageView: boolean = true;

	get movies(): PlexMovieDTO[] {
		return this.library?.movies ?? [];
	}

	get server(): PlexServerDTO | null {
		return this.activeAccount?.plexServers.find((x) => x.id === this.library?.plexServerId) ?? null;
	}

	get activeAccountId(): number {
		return this.activeAccount?.id ?? 0;
	}

	get libraryType(): PlexMediaType {
		return this.library?.type ?? PlexMediaType.Unknown;
	}

	refreshLibrary(): void {
		this.isLoading = true;

		refreshPlexLibrary(this.libraryId, this.activeAccountId).subscribe((data) => {
			this.library = data;
			this.isLoading = false;
		});
	}

	getLibrary(): void {
		this.isLoading = true;
		getPlexLibrary(this.libraryId, this.activeAccountId).subscribe((data) => {
			this.library = data;
			this.isLoading = false;
		});
	}

	created(): void {
		this.libraryId = +this.$route.params.id;
		SettingsService.getActiveAccount().subscribe((data) => {
			Log.debug(`MoviesDetail => ${data?.id}`, data);
			this.activeAccount = data ?? null;
			this.getLibrary();
		});
	}
}
</script>
