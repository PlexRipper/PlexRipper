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
		class="season-table"
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
			<v-checkbox :indeterminate="isTvShowIndeterminate(item.id)" @change="tvShowSelected(item.id, $event)" />
		</template>

		<template v-slot:expanded-item="{ headers, item }">
			<td :colspan="24">
				<v-list>
					<!-- Season -->
					<v-list-group v-for="(season, index) in item.seasons" :key="index" sub-group :value="false">
						<!-- The season header -->
						<template v-slot:activator>
							<v-checkbox
								:value="seasonValue(item.id, season.id)"
								@click.self.stop="seasonSelected(item.id, season.id, !seasonValue(item.id, season.id))"
							></v-checkbox>
							<v-list-item-title>{{ season.title }}</v-list-item-title>
						</template>
						<!-- Episodes -->
						<v-list-item v-for="(episode, y) in season.episodes" :key="y" style="padding-left: 90px">
							<v-list-item-action>
								<v-checkbox></v-checkbox>
							</v-list-item-action>
							<v-list-item-title> {{ episode.title }} </v-list-item-title>
						</v-list-item>
					</v-list-group>
				</v-list>
			</td>
		</template>
		<template v-slot:item.actions="{ item }">
			<v-icon small @click="downloadMovie(item)">
				mdi-download
			</v-icon>
		</template>
	</v-data-table>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import DownloadService from '@service/downloadService';
import { PlexAccountDTO, PlexTvShowDTO } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { downloadPlexMovie } from '@/types/api/plexDownloadApi';

interface ITvShowSelector {
	id: number;
	selected: boolean;
	indeterminate: boolean;
	seasons: ISeasonSelector[];
}

interface ISeasonSelector {
	id: number;
	selected: boolean;
	episodes: IEpisodeSelector[];
}

interface IEpisodeSelector {
	id: number;
	selected: boolean;
}

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

	@Prop({ required: true, type: Boolean, default: true })
	readonly loading!: Boolean;

	expanded: string[] = [];
	singleExpand: boolean = false;
	selected: ITvShowSelector[] = [];

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
		let index = this.selected.findIndex((x) => x.id === id);
		if (index === -1) {
			// Has not been added
			this.selected.push({ id, indeterminate: false, selected: state, seasons: [] });
			index = this.selected.length - 1;
		} else {
			this.selected[index].selected = state;
		}

		Log.debug(this.selected);
	}

	isTvShowIndeterminate(tvShowId: number): boolean {
		return this.selected.find((x) => x.id === tvShowId)?.indeterminate ?? false;
	}

	tvShowValue(tvShowId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		if (tvShowindex === -1) {
			return false;
		}
		return this.selected[tvShowindex].selected;
	}

	seasonValue(tvShowId: number, seasonId: number): boolean {
		const tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		if (tvShowindex === -1) {
			return false;
		}
		const seasonIndex = this.selected[tvShowindex]?.seasons.findIndex((x) => x.id === seasonId) ?? -1;
		if (seasonIndex === -1) {
			return false;
		}
		return this.selected[tvShowindex].seasons[seasonIndex].selected;
	}

	seasonSelected(tvShowId: number, seasonId: number, state: boolean): void {
		let tvShowindex = this.selected.findIndex((x) => x.id === tvShowId);
		let seasonIndex = this.selected[tvShowindex]?.seasons.findIndex((x) => x.id === seasonId) ?? -1;
		if (tvShowindex === -1) {
			// Has not been added
			this.selected.push({ id: tvShowId, selected: state, indeterminate: state, seasons: [] });
			tvShowindex = this.selected.length - 1;
		}

		if (seasonIndex === -1) {
			// Has not been added
			this.selected[tvShowindex].seasons.push({ id: seasonId, selected: state, episodes: [] });
			seasonIndex = this.selected[tvShowindex].seasons.length - 1;
		} else {
			this.selected[tvShowindex].seasons[seasonIndex].selected = state;
		}
		this.selected[tvShowindex].indeterminate = state;

		Log.debug(this.selected);
	}

	downloadMovie(item: PlexTvShowDTO): void {
		downloadPlexMovie(item?.id ?? 0, this.activeAccount?.id ?? 0).subscribe(() => {
			DownloadService.fetchDownloadList();
		});
	}
}
</script>

<style lang="scss" scoped>
.text-start {
	background: floralwhite !important;
	max-width: 40px !important;
}
</style>
