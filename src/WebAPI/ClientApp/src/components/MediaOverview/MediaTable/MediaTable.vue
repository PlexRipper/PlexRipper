<template>
	<v-row justify="center" class="media-table flex-nowrap" no-gutters>
		<v-col>
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
			<v-row no-gutters :class="['media-table-content', detailMode ? 'detail-mode' : '']">
				<perfect-scrollbar ref="scrollbarmediatable" :options="{ suppressScrollX: true }">
					<v-col id="media-table-body" class="col px-0">
						<template v-for="(parentItem, i) in items">
							<v-lazy
								:key="i"
								:options="{
									threshold: 0.25,
								}"
								:min-height="50"
								:data-title="parentItem.title"
								transition="scroll-x-reverse-transition"
							>
								<v-treeview
									selectable
									selected-color="red"
									selection-type="leaf"
									hoverable
									expand-icon="mdi-chevron-down"
									:items="[parentItem]"
									:load-children="getMedia"
									:open-all="detailMode"
									transition
									item-key="key"
									item-text="title"
									class="media-table-row"
									@input="updateSelected(i, $event)"
								>
									<template #label="{ item }">
										<v-row class="media-table-content-row" align="center">
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
											<v-col cols="auto" class="py-0">
												<v-sheet width="70" class="no-background text-center">
													<p-btn
														button-type="download"
														:loading="isLoading(item.key)"
														icon-mode
														@click="downloadMedia(item)"
													></p-btn>
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
		<alphabet-navigation v-if="!hideNavigation" :items="items" container-ref="scrollbarmediatable" />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { DownloadMediaDTO, DownloadTaskCreationProgress, PlexMediaType } from '@dto/mainApi';
import IMediaTableHeader from '@interfaces/IMediaTableHeader';
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import { Dictionary } from 'typescript-collections';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';
import ITreeViewItem from './types/ITreeViewItem';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
		AlphabetNavigation,
	},
})
export default class MediaTable extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ required: false, type: Boolean })
	readonly hideNavigation!: boolean;

	@Prop({ type: Boolean })
	readonly detailMode!: boolean;

	selected: string[] = [];

	expanded: string[] = [];

	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	visible: boolean[] = [];
	loadingButtons: string[] = [];

	selectedDict: Dictionary<number, string[]> = new Dictionary<number, string[]>();

	@Watch('items')
	updateVisible(): void {
		this.items.forEach(() => this.visible.push(false));
	}

	get getLeafs(): string[] {
		if (this.mediaType === PlexMediaType.Movie) {
			return this.items.map((x) => x.key);
		}
		return this.items.map((x) => x.children?.map((y) => y.children?.map((z) => z.key) ?? []) ?? [])?.flat(2) ?? [];
	}

	get isIndeterminate(): boolean {
		return this.getLeafs.length !== this.selected.length && this.selected.length > 0;
	}

	get containerRef(): any {
		return this.$refs.scrollbar;
	}

	isLoading(key: string): boolean {
		return this.loadingButtons.some((x) => x === key);
	}

	updateSelected(i: number, selected: string[]) {
		if (i === 0) {
			this.$emit('selected', selected);
		} else {
			this.selectedDict.setValue(i, selected);
			this.$emit('selected', this.selectedDict.values().flat(1));
		}
	}

	selectAll(state: boolean): void {
		this.updateSelected(0, state ? this.getLeafs : []);
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
				value: 'mediaSize',
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

	/*
	A promise is send to the parent, which will resolve once the data is available. After which the node expands.
	 */
	getMedia(item: ITreeViewItem): Promise<ITreeViewItem> {
		return new Promise((resolve) => this.$emit('request-media', { mediaId: item.id, resolve }));
	}

	async downloadMedia(item: ITreeViewItem): Promise<void> {
		// Set as currently loading.
		this.loadingButtons.push(item.key);
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
				if (item.children?.length === 0) {
					await this.getMedia(item);
				}
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.Season:
				downloadCommand.mediaIds.push(item.id);
				break;
			case PlexMediaType.Episode:
				downloadCommand.mediaIds.push(item.id);
				break;
			default:
				return;
		}
		// Set finished loading
		const i = this.loadingButtons.findIndex((x) => x === item.key);
		if (i > -1) {
			this.loadingButtons.splice(i, 1);
		} else {
			this.loadingButtons = [];
		}
		this.$emit('download', downloadCommand);
	}
}
</script>
