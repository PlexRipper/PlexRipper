<template>
	<v-row justify="center">
		<v-col cols="12">
			<media-table :headers="getHeaders" :items="items" :media-type="getType" @download="downloadMovie" />
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import { PlexMediaType, PlexMovieDTO } from '@dto/mainApi';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import IMediaId from '@mediaOverview/MediaTable/types/IMediaId';

@Component({
	components: {
		MediaTable,
	},
})
export default class MovieTable extends Vue {
	@Prop({ required: true, type: Number })
	readonly accountId!: number;

	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	expanded: string[] = [];
	singleExpand: boolean = false;
	selected: number[] = [];

	showDialog: boolean = false;

	get getType(): PlexMediaType {
		return PlexMediaType.Movie;
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

	downloadMovie(mediaId: IMediaId): void {
		this.$emit('download', mediaId);
	}
}
</script>
