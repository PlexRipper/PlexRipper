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
import { Component, Vue } from 'vue-property-decorator';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';
import IPlexAccount from '@dto/IPlexAccount';
import SettingsService from '@service/settingsService';
import Log from 'consola';
import IPlexLibrary from '@dto/IPlexLibrary';
import IPlexServer from '@dto/IPlexServer';
import IPlexTvShow from '@dto/IPlexTvShow';
import TvShowTable from './components/TvShowTable.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

@Component<TvShowsDetail>({
	components: {
		LoadingSpinner,
		TvShowTable,
	},
})
export default class TvShowsDetail extends Vue {
	activeAccount: IPlexAccount | null = null;

	libraryId: number = 0;

	library: IPlexLibrary | null = null;

	isLoading: boolean = true;

	get tvshows(): IPlexTvShow[] {
		return this.library?.tvShows ?? [];
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
