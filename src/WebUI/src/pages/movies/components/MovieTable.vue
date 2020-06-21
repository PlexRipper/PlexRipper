<template>
	<v-data-table
		fixed-header
		show-select
		disable-pagination
		hide-default-footer
		:headers="getHeaders"
		:items="movies"
		:server-items-length="movies.length"
		:dark="$vuetify.theme.dark"
		:loading="loading"
	/>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { IPlexMovie } from '@dto/IPlexMovie';
import { DataTableHeader } from 'vuetify/types';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class MovieTable extends Vue {
	@Prop({ required: true, type: Array as () => IPlexMovie[] })
	readonly movies!: IPlexMovie[];

	@Prop({ required: true, type: Boolean })
	readonly loading!: Boolean;

	get getHeaders(): DataTableHeader<IPlexMovie>[] {
		return [
			{
				text: 'Id',
				value: 'id',
			},
			{
				text: 'Title',
				value: 'title',
			},
			{
				text: 'Year',
				value: 'year',
			},
			{
				text: 'Added At',
				value: 'addedAt',
			},
			{
				text: 'Updated At',
				value: 'updatedAt',
			},
		];
	}
}
</script>
