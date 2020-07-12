<template>
	<v-container>
		<v-row>
			<v-col cols="6">
				<p>Movies detail {{ libraryId }}</p>
			</v-col>
			<v-col cols="auto">
				<v-btn @click="refreshLibraryAsync">Refresh Library</v-btn>
			</v-col>
			<v-col cols="12">
				<movie-table :movies="movies" :loading="isLoading" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import * as PlexLibraryApi from '@api/plexLibraryApi';
import { IPlexMovie } from '@dto/IPlexMovie';
import MovieTable from './components/MovieTable.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { UserStore } from '@/store/';

@Component({
	components: {
		LoadingSpinner,
		MovieTable,
	},
})
export default class MoviesDetail extends Vue {
	libraryId: number = 0;

	movies: IPlexMovie[] = [];

	isLoading: boolean = true;

	async refreshLibraryAsync(): Promise<void> {
		this.isLoading = true;
		const libraryData = await PlexLibraryApi.refreshPlexLibraryAsync(this.libraryId, UserStore.getAccountId);
		if (libraryData.type === 'movie') {
			this.movies = libraryData.movies ?? [];
		}
		this.isLoading = false;
	}

	async getLibraryAsync(): Promise<void> {
		this.isLoading = true;
		const libraryData = await PlexLibraryApi.getPlexLibraryAsync(this.libraryId, UserStore.getAccountId);
		if (libraryData.type === 'movie') {
			this.movies = libraryData.movies ?? [];
		}
		this.isLoading = false;
	}

	async mounted(): Promise<void> {
		this.libraryId = +this.$route.params.id;
		await this.getLibraryAsync();
	}
}
</script>
