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
					{{ headers[0].text }}
				</v-col>
				<v-spacer />
				<!-- Other columns -->
				<v-col v-for="(header, i) in headers.slice(1, headers.length)" :key="i" cols="auto">
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
						<v-treeview
							v-model="selected"
							selectable
							selected-color="red"
							selection-type="leaf"
							hoverable
							expand-icon="mdi-chevron-down"
							:items="items"
							transition
							item-key="key"
							item-text="title"
						>
							<template #label="{ item }">
								<v-row class="media-table-content-row">
									<!-- Title -->
									<v-col cols="4" class="title-column">
										{{ item[headers[0].value] }}
									</v-col>
									<v-spacer />
									<!-- Other columns -->
									<v-col v-for="(header, index) in headers.slice(1, headers.length)" :key="index" cols="auto">
										<v-sheet :width="header.width" :max-width="header.width" class="no-background">
											<date-time v-if="header.type === 'date'" :text="item[header.value]" :time="false" short-date />
											<file-size v-else-if="header.type === 'data'" :size="item[header.value]" />
											<span v-else>{{ item[header.value] }}</span>
										</v-sheet>
									</v-col>
									<!-- Actions -->
									<v-col cols="auto">
										<v-sheet width="70" class="no-background text-center">
											<v-icon small @click="downloadMedia(item.id, item.type)"> mdi-download </v-icon>
										</v-sheet>
									</v-col>
								</v-row>
							</template>
						</v-treeview>
					</v-col>
				</perfect-scrollbar>
			</v-row>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadTaskCreationProgress, PlexMediaType, PlexTvShowDTO } from '@dto/mainApi';
import IMediaTableHeader from '@interfaces/IMediaTableHeader';
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import IMediaId from '@mediaOverview/MediaTable/types/IMediaId';
import ITreeViewItem from './types/ITreeViewItem';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
	},
})
export default class MediaTable extends Vue {
	@Prop({ required: true, type: Array as () => IMediaTableHeader<PlexTvShowDTO>[] })
	readonly headers!: IMediaTableHeader[];

	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	expanded: string[] = [];
	selected: string[] = [];

	showDialog: boolean = false;
	openDownloadPreviews: number[] = [];

	progress: DownloadTaskCreationProgress | null = null;

	get getLeafs(): string[] {
		if (this.mediaType === PlexMediaType.Movie) {
			return this.items.map((x) => x.key);
		}
		return this.items.map((x) => x.children?.map((y) => y.children?.map((z) => z.key))).flat(2);
	}

	get isIndeterminate(): boolean {
		return this.getLeafs.length !== this.selected.length && this.selected.length > 0;
	}

	selectAll(state: boolean): void {
		if (state) {
			this.selected = this.getLeafs;
		} else {
			this.selected = [];
		}
	}

	downloadMedia(mediaId: number, type: PlexMediaType): void {
		this.$emit('download', { id: mediaId, type } as IMediaId);
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
