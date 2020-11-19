<template>
	<v-row class="table-overview" justify="center" no-gutters>
		<v-col cols="12">
			<media-table :headers="getHeaders" :items="items" :media-type="getType" @download="downloadTvShows" />
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import type { PlexAccountDTO } from '@dto/mainApi';
import { DownloadTaskCreationProgress, PlexMediaType, PlexTvShowDTO } from '@dto/mainApi';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import IMediaId from '@mediaOverview/MediaTable/types/IMediaId';

@Component({
	components: {
		MediaTable,
	},
})
export default class TVShowsTable extends Vue {
	@Prop({ required: true, type: Object as () => PlexAccountDTO })
	readonly activeAccount!: PlexAccountDTO;

	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	expanded: string[] = [];
	singleExpand: boolean = false;
	selected: number[] = [];

	showDialog: boolean = false;
	downloadPreview: ITreeViewItem[] = [];
	downloadPreviewType: PlexMediaType = PlexMediaType.None;
	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	get getType(): PlexMediaType {
		return PlexMediaType.TvShow;
	}

	get getHeaders(): DataTableHeader<PlexTvShowDTO>[] {
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
				type: 'date',
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
				width: 150,
				type: 'date',
			},
		];
	}

	downloadTvShows(mediaId: IMediaId): void {
		this.$emit('download', mediaId);
	}
}
</script>
