<template>
	<v-row>
		<v-col cols="12">
			<p>Movies detail {{ libraryId }}</p>
		</v-col>
		<v-col cols="12">
			<movie-table :movies="movies" :loading="isLoading" />
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Log from 'consola';
import * as PlexLibraryApi from '@api/plexLibraryApi';
import { IPlexMovie } from '@dto/IPlexMovie';
import MovieTable from './components/MovieTable.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

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

	async created(): Promise<void> {
		this.libraryId = +this.$route.params.id;
		const libraryData = await PlexLibraryApi.getPlexLibraryAsync(this.libraryId);
		if (libraryData.type === 'movie') {
			this.movies = libraryData.movies ?? [];
		}
		this.isLoading = false;
		Log.debug(libraryData);
	}
}
</script>
