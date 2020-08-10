<template>
	<v-data-table
		v-model="selected"
		fixed-header
		show-select
		disable-pagination
		hide-default-footer
		:headers="getHeaders"
		:items="tvshows"
		:server-items-length="tvshows.length"
		:dark="$vuetify.theme.dark"
		:loading="loading"
		show-expand
		:expanded.sync="expanded"
	>
		<template v-slot:top>
			<v-toolbar flat>
				<v-toolbar-title>Expandable Table</v-toolbar-title>
				<v-spacer></v-spacer>
				<v-switch v-model="singleExpand" label="Single expand" class="mt-2"></v-switch>
			</v-toolbar>
		</template>

		<template v-slot:item.data-table-select="{ item }">
			<v-checkbox
				:value="tvShowValue(item.id)"
				:indeterminate="isTvShowIndeterminate(item.id)"
				@click="tvShowSelected(item.id, !item.selected)"
			/>
		</template>

		<template v-slot:expanded-item="{ headers, item }">
			<td :colspan="24">
				<v-list>
					<!-- Season -->
					<v-list-group v-for="(season, index) in item.seasons" :key="`${item.id}-${index}`" sub-group>
						<!-- The season header -->
						<template v-slot:activator>
							<v-checkbox
								:value="seasonValue(item.id, season.id)"
								:indeterminate="isSeasonIndeterminate(item.id, season.id)"
								@click.self.stop="seasonSelected(item.id, season.id, !seasonValue(item.id, season.id))"
							></v-checkbox>
							<v-list-item-title>{{ season.title }}</v-list-item-title>
						</template>
						<!-- Episodes -->
						<v-list-item v-for="(episode, y) in season.episodes" :key="`${item.id}-${index}-${y}`" style="padding-left: 90px">
							<v-list-item-action>
								<v-checkbox
									:value="episodeValue(item.id, season.id, episode.id)"
									@click.self.stop="
										episodeSelected(item.id, season.id, episode.id, !episodeValue(item.id, season.id, episode.id))
									"
								></v-checkbox>
							</v-list-item-action>
							<v-list-item-title> {{ episode.title }} </v-list-item-title>
						</v-list-item>
					</v-list-group>
				</v-list>
			</td>
		</template>
		<!-- Tv Show actions -->
		<template v-slot:item.actions="{ item }">
			<v-icon small @click="downloadMovie(item)">
				mdi-download
			</v-icon>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import DownloadService from '@service/downloadService';
import { PlexAccountDTO, PlexTvShowDTO } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadPlexMovie } from '@/types/api/plexDownloadApi';
import { ITvShowSelector } from '../types/iTvShowSelector';
@Component({
	components: {
		LoadingSpinner,
	},
})
export default class TVShowsTable extends Vue {
	@Prop({ required: true, type: Object as () => PlexAccountDTO })
	readonly activeAccount!: PlexAccountDTO;

	@Prop({ required: true, type: Array as () => PlexTvShowDTO[] })
	readonly tvshows!: PlexTvShowDTO[];

	@Prop({ required: true, type: Array as () => ITvShowSelector[] })
	readonly selected!: ITvShowSelector[];

	@Prop({ required: true, type: Boolean, default: true })
	readonly loading!: Boolean;

	expanded: string[] = [];
	singleExpand: boolean = false;

	get getHeaders(): DataTableHeader<PlexTvShowDTO>[] {
		return [
			// {
			// 	text: 'Id',
			// 	value: 'id',
			// },
			{
				text: 'Title',
				value: 'title',
				width: 500,
			},
			{
				text: 'Year',
				value: 'year',
				width: 50,
			},
			{
				text: 'Added At',
				value: 'addedAt',
				width: 150,
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
				width: 150,
			},
			{
				text: 'Actions',
				value: 'actions',
				width: 50,
				sortable: false,
			},
		];
	}

	tvShowSelected(id: number, state: boolean): void {
		const index = this.selected.findIndex((x) => x.id === id);

		// Set all seasons
		this.selected[index].seasons.forEach((season) => this.seasonSelected(id, season.id, state));
		this.selected[index].selected = state;
	}

	isTvShowIndeterminate(tvShowId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		const numberOfSelected = this.selected[tvShowindex].seasons.filter((season) => {
			return this.isSeasonIndeterminate(tvShowId, season.id) || season.selected;
		}).length;
		return !(numberOfSelected === 0 || numberOfSelected === this.selected[tvShowindex].seasons.length);
	}

	tvShowValue(tvShowId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		if (tvShowindex === -1) {
			return false;
		}

		const numberOfSelected = this.selected[tvShowindex].seasons.filter((season) => {
			return this.seasonValue(tvShowId, season.id);
		}).length;

		// Set TvShow selected state based on how the seasons are selected
		this.selected[tvShowindex].selected = numberOfSelected === this.selected[tvShowindex].seasons.length;

		return this.selected[tvShowindex].selected;
	}

	seasonValue(tvShowId: number, seasonId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		const seasonIndex = this.selected[tvShowindex].seasons.findIndex((x) => x.id === seasonId);

		const numberOfSelected = this.selected[tvShowindex].seasons[seasonIndex].episodes.filter((x) => x.selected).length;

		// Set Season selected state based on how the episodes are selected
		this.selected[tvShowindex].seasons[seasonIndex].selected =
			numberOfSelected === this.selected[tvShowindex].seasons[seasonIndex].episodes.length;

		return this.selected[tvShowindex].seasons[seasonIndex].selected;
	}

	isSeasonIndeterminate(tvShowId: number, seasonId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		const seasonIndex = this.selected[tvShowindex].seasons.findIndex((x) => x.id === seasonId);

		const numberOfSelected = this.selected[tvShowindex].seasons[seasonIndex].episodes.filter((x) => x.selected).length;

		return !(numberOfSelected === 0 || numberOfSelected === this.selected[tvShowindex].seasons[seasonIndex].episodes.length);
	}

	seasonSelected(tvShowId: number, seasonId: number, state: boolean): void {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		const seasonIndex = this.selected[tvShowindex].seasons.findIndex((x) => x.id === seasonId);

		this.selected[tvShowindex].seasons[seasonIndex].episodes.forEach((episode) =>
			this.episodeSelected(tvShowId, seasonId, episode.id, state),
		);
		this.selected[tvShowindex].seasons[seasonIndex].selected = state;
		this.$emit('selected', this.selected);
	}

	episodeValue(tvShowId: number, seasonId: number, episodeId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId) ?? -1;
		const seasonIndex = this.selected[tvShowindex]?.seasons.findIndex((x) => x.id === seasonId) ?? -1;
		const episodeIndex = this.selected[tvShowindex]?.seasons[seasonIndex]?.episodes.findIndex((x) => x.id === episodeId) ?? -1;

		if (tvShowindex === -1 || seasonIndex === -1 || episodeIndex === -1) {
			return false;
		}

		return this.selected[tvShowindex].seasons[seasonIndex].episodes[episodeIndex].selected;
	}

	episodeSelected(tvShowId: number, seasonId: number, episodeId: number, state: boolean): void {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId) ?? -1;
		const seasonIndex = this.selected[tvShowindex]?.seasons.findIndex((x) => x.id === seasonId) ?? -1;
		const episodeIndex = this.selected[tvShowindex]?.seasons[seasonIndex]?.episodes.findIndex((x) => x.id === episodeId) ?? -1;

		if (tvShowindex === -1 || seasonIndex === -1 || episodeIndex === -1) {
			return;
		}

		this.selected[tvShowindex].seasons[seasonIndex].episodes[episodeIndex].selected = state;
		this.$emit('selected', this.selected);
	}

	downloadMovie(item: PlexTvShowDTO): void {
		downloadPlexMovie(item?.id ?? 0, this.activeAccount?.id ?? 0).subscribe(() => {
			DownloadService.fetchDownloadList();
		});
	}
}
</script>

<style lang="scss">
tr.v-data-table__selected {
	background: none !important;
}
</style>
