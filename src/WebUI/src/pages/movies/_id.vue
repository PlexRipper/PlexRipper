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
				<movie-table :movies="movies" :active-account="activeAccount" :loading="isLoading" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';
import IPlexMovie from '@dto/IPlexMovie';
import IPlexAccount from '@dto/IPlexAccount';
import SettingsService from '@service/settingsService';
import Log from 'consola';
import IPlexLibrary from '@dto/IPlexLibrary';
import IPlexServer from '@dto/IPlexServer';
import MovieTable from './components/MovieTable.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

@Component<MoviesDetail>({
	components: {
		LoadingSpinner,
		MovieTable,
	},
})
export default class MoviesDetail extends Vue {
	activeAccount: IPlexAccount | null = null;

	libraryId: number = 0;

	library: IPlexLibrary | null = null;

	isLoading: boolean = true;

	get movies(): IPlexMovie[] {
		return this.library?.movies ?? [];
	}

	get server(): IPlexServer | null {
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
