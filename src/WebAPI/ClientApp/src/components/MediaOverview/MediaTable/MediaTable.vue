<template>
	<v-row justify="center" class="media-table">
		<v-col cols="12">
			<!-- Table Headers -->
			<v-row class="media-table-header">
				<!-- Checkbox -->
				<v-col class="ml-6 select-all-check" style="max-width: 50px">
					<v-checkbox :indeterminate="isIndeterminate" color="red" @change="selectAll($event)"></v-checkbox>
				</v-col>
				<!-- Title -->
				<v-col cols="4" class="title-column">
					{{ getHeaders[0].text }}
				</v-col>
				<v-spacer />
				<!-- Other columns -->
				<v-col v-for="(header, i) in getHeaders.slice(1, getHeaders.length)" :key="i" cols="auto">
					<v-sheet :width="header.width" :max-width="header.width" class="no-background">{{ header.text }}</v-sheet>
				</v-col>
				<!-- Actions -->
				<v-col cols="auto" class="text-center">
					<v-sheet width="70" class="no-background">Actions</v-sheet>
				</v-col>
			</v-row>
			<!-- TreeView Table -->
			<v-row no-gutters class="media-table-content">
				<perfect-scrollbar>
					<v-col class="col-12 px-0">
						<template v-for="(x, y) in tempItems.length">
							<v-lazy
								:key="y"
								:options="{
									threshold: 0.5,
								}"
								:min-height="50"
							>
								<v-treeview
									selectable
									selected-color="red"
									selection-type="leaf"
									hoverable
									expand-icon="mdi-chevron-down"
									:items="tempItems.slice(y, y + 1)"
									:load-children="getMedia"
									transition
									open-on-click
									item-key="key"
									item-text="title"
									@input="updateSelected"
								>
									<template #label="{ item }">
										<v-row class="media-table-content-row">
											<!-- Title -->
											<v-col cols="4" class="title-column">
												{{ item[getHeaders[0].value] }}
											</v-col>
											<v-spacer />
											<!-- Other columns -->
											<v-col v-for="(header, index) in getHeaders.slice(1, getHeaders.length)" :key="index" cols="auto">
												<v-sheet :width="header.width" :max-width="header.width" class="no-background">
													<date-time v-if="header.type === 'date'" :text="item[header.value]" :time="false" short-date />
													<file-size v-else-if="header.type === 'data'" :size="item[header.value]" />
													<span v-else>{{ item[header.value] }}</span>
												</v-sheet>
											</v-col>
											<!-- Actions -->
											<v-col cols="auto">
												<v-sheet width="70" class="no-background text-center">
													<v-icon small @click="downloadMedia(item)"> mdi-download </v-icon>
												</v-sheet>
											</v-col>
										</v-row>
									</template>
								</v-treeview>
							</v-lazy>
						</template>
					</v-col>
				</perfect-scrollbar>
			</v-row>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { DownloadMediaDTO, DownloadTaskCreationProgress, PlexMediaType } from '@dto/mainApi';
import IMediaTableHeader from '@interfaces/IMediaTableHeader';
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import { getTvShow } from '@api/mediaApi';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import ITreeViewItem from './types/ITreeViewItem';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
	},
})
export default class MediaTable extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	selected: string[] = [];

	expanded: string[] = [];

	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	visible: boolean[] = [];
	active: boolean[] = [];
	tempItems: ITreeViewItem[] = [];

	@Watch('tempItems')
	updateVisible(): void {
		this.tempItems.forEach(() => this.visible.push(false));
	}

	get getLeafs(): string[] {
		if (this.mediaType === PlexMediaType.Movie) {
			return this.tempItems.map((x) => x.key);
		}
		return this.tempItems.map((x) => x.children?.map((y) => y.children?.map((z) => z.key))).flat(2);
	}

	get isIndeterminate(): boolean {
		return this.getLeafs.length !== this.selected.length && this.selected.length > 0;
	}

	updateSelected(selected: string[]) {
		this.$emit('selected', selected);
	}

	selectAll(state: boolean): void {
		this.updateSelected(state ? this.getLeafs : []);
	}

	get getHeaders(): IMediaTableHeader[] {
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
				text: 'Size',
				value: 'size',
				width: 100,
				type: 'data',
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

	getMedia(item: ITreeViewItem): Promise<ITreeViewItem> {
		return getTvShow(item.id)
			.toPromise()
			.then((response) => {
				const convert = Convert.tvShowsToTreeViewItems([response])[0];
				item.children.push(...convert.children);
				const i = this.tempItems.findIndex((x) => x.key === item.key);
				this.tempItems.splice(i, 1, item);
				return item;
			});
	}

	downloadMedia(item: ITreeViewItem): void {
		const downloadCommand: DownloadMediaDTO = {
			type: item.type,
			mediaIds: [],
			libraryId: 0,
			plexAccountId: 0,
		};
		switch (item.type) {
			case PlexMediaType.Movie:
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.TvShow:
				downloadCommand.mediaIds = item.children.flatMap((x) => x.children.flatMap((y) => y.id));
				break;
			case PlexMediaType.Season:
				downloadCommand.mediaIds = item.children.flatMap((x) => x.id);
				break;
			case PlexMediaType.Episode:
				downloadCommand.mediaIds.push(item.id);
				break;
			default:
				return;
		}

		this.$emit('download', downloadCommand);
	}

	created(): void {
		// This is needed to refresh the TreeView when a node is expanded.
		// We edit the copy instead of the prop
		this.tempItems = this.items;
	}
}
</script>

<style lang="scss">
.v-treeview.theme--dark {
	.v-treeview-node {
		border-top: 0.888889px solid rgba(255, 255, 255, 0.377);
	}
}

.table-header-dark {
	border-top: 0.888889px solid rgba(255, 255, 255, 0.377);
}

.select-all-check {
	.v-input--selection-controls {
		padding: 0 0 0 2px !important;
		margin: 0 !important;
	}
}
</style>
