<template>
	<v-row justify="center">
		<v-col cols="12">
			<media-table
				:headers="getHeaders"
				:items="getItems"
				:media-type="getType"
				@download="openDownloadConfirmationDialog($event.itemId, $event.type)"
			/>
		</v-col>
		<!-- The "Are you sure" dialog -->
		<v-col cols="12">
			<v-dialog v-model="showDialog" :max-width="500" scrollable>
				<v-card v-if="progress === null">
					<v-card-title> Are you sure? </v-card-title>
					<v-card-subtitle>
						<p>Plex Ripper will start downloading the following:</p>
					</v-card-subtitle>
					<!-- Show Download Task Preview -->
					<v-card-text>
						<v-treeview :items="downloadPreview" item-text="title" :open.sync="openDownloadPreviews" open-all></v-treeview>
					</v-card-text>
					<v-divider></v-divider>

					<v-card-actions>
						<v-btn large @click="showDialog = false"> Cancel </v-btn>
						<v-spacer></v-spacer>
						<v-btn color="success" large @click="downloadMovie()"> Yes! </v-btn>
					</v-card-actions>
				</v-card>

				<!-- Download Task Creation Progressbar -->
				<v-card v-else>
					<v-card-title class="justify-center">
						Creating download tasks {{ progress.current }} of {{ progress.total }}
					</v-card-title>
					<v-card-text>
						<progress-component :percentage="progress.percentage" text="" />
					</v-card-text>
				</v-card>
			</v-dialog>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import { PlexMediaType, PlexMovieDTO } from '@dto/mainApi';
import { clone } from 'lodash';
import ProgressComponent from '@components/ProgressComponent.vue';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/iTreeViewItem';

@Component({
	components: {
		MediaTable,
		ProgressComponent,
	},
})
export default class MovieTable extends Vue {
	@Prop({ required: true, type: Number })
	readonly accountId!: number;

	@Prop({ required: true, type: Array as () => PlexMovieDTO[] })
	readonly movies!: PlexMovieDTO[];

	expanded: string[] = [];
	singleExpand: boolean = false;
	selected: number[] = [];

	showDialog: boolean = false;
	downloadPreview: ITreeViewItem[] = [];
	downloadPreviewType: PlexMediaType = PlexMediaType.None;
	openDownloadPreviews: number[] = [];

	get getType(): PlexMediaType {
		return PlexMediaType.Movie;
	}

	get getItems(): ITreeViewItem[] {
		const items: ITreeViewItem[] = [];
		this.movies.forEach((movie: PlexMovieDTO) => {
			if (movie) {
				// Add tvShow
				items.push({
					id: movie.id,
					key: `${movie.id}`,
					title: movie.title ?? '',
					year: movie.year,
					type: PlexMediaType.Movie,
					item: movie,
					children: [],
					addedAt: movie.addedAt ?? '',
					updatedAt: movie.updatedAt ?? '',
				});
			}
		});
		return items;
	}

	get getHeaders(): DataTableHeader<PlexMovieDTO>[] {
		return [
			// {
			// 	text: 'Id',
			// 	value: 'id',
			// },
			{
				text: 'Title',
				value: 'title',
				width: 400,
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
		];
	}

	get getLeafs(): number[] {
		return this.getItems.map((x) => x.children?.map((y) => y.children?.map((z) => z.id))).flat(2);
	}

	createPreview(itemId: number, type: PlexMediaType): ITreeViewItem[] {
		const result: ITreeViewItem[] = [];
		this.openDownloadPreviews = [];

		if (type === 'Movie') {
			const movie: ITreeViewItem | undefined = this.getItems.find((x) => x.id === itemId);
			if (movie) {
				// Ensure all nodes are open
				this.openDownloadPreviews.push(movie.id);
				movie.children.forEach((season) => {
					this.openDownloadPreviews.push(season.id);
				});
				result.push(clone(movie));
			}
		}
		return result;
	}

	openDownloadConfirmationDialog(itemId: number, type: PlexMediaType): void {
		this.downloadPreview = this.createPreview(itemId, type);
		this.downloadPreviewType = type;
		this.showDialog = true;
	}

	downloadMovie(): void {
		let itemId = 0;
		if (this.downloadPreviewType === 'Movie') {
			itemId = this.downloadPreview[0].id;
		}

		this.$emit('download', itemId);
	}
}
</script>
