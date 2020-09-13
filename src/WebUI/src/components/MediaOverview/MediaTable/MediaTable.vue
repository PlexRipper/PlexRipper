<template>
	<v-row justify="center">
		<v-col cols="12">
			<!-- Table Headers -->
			<v-row :class="$classNames({ 'table-header-dark': $vuetify.theme.dark })" style="height: 50px; margin: 0">
				<v-col class="ml-6 select-all-check" style="max-width: 50px">
					<v-checkbox :indeterminate="isIndeterminate" color="red" @change="selectAll($event)"></v-checkbox>
				</v-col>
				<v-col v-for="(header, index) in headers" :key="index" :class="$classNames({ 'col-auto': index > 0 }, 'col')">
					<v-sheet :width="header.width" :max-width="header.width">{{ header.text }}</v-sheet>
				</v-col>
				<v-col cols="auto" class="text-center">
					<v-sheet width="70">Actions</v-sheet>
				</v-col>
			</v-row>
			<!-- TreeView Table -->
			<v-row no-gutters>
				<v-col>
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
						<template v-slot:label="{ item }">
							<v-row class="ml-2">
								<v-col v-for="(header, index) in headers" :key="index" :class="$classNames({ 'col-auto': index > 0 }, 'col')">
									<v-sheet :width="header.width" :max-width="header.width">{{ item[header.value] }}</v-sheet>
								</v-col>
								<!-- Actions -->
								<v-col cols="auto">
									<v-sheet width="70" style="text-align: center">
										<v-icon small @click="openDownloadConfirmationDialog(item.id, item.type)"> mdi-download </v-icon>
									</v-sheet>
								</v-col>
							</v-row>
						</template>
					</v-treeview>
				</v-col>
			</v-row>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify/types';
import { DownloadTaskCreationProgress, PlexMediaType, PlexTvShowDTO } from '@dto/mainApi';
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import ITreeViewItem from './types/iTreeViewItem';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
	},
})
export default class MediaTable extends Vue {
	@Prop({ required: true, type: Array as () => DataTableHeader<PlexTvShowDTO>[] })
	readonly headers!: DataTableHeader[];

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

	openDownloadConfirmationDialog(itemId: number, type: PlexMediaType): void {
		this.$emit('download', { itemId, type });
	}
}
</script>

<style lang="scss">
.v-treeview.theme--dark {
	.v-treeview-node {
		background: #1e1e1e !important;
		border-top: 0.888889px solid rgba(255, 255, 255, 0.377);
	}
}

.table-header-dark {
	background: #1e1e1e;
	border-top: 0.888889px solid rgba(255, 255, 255, 0.377);
}

.select-all-check {
	.v-input--selection-controls {
		padding: 0 0 0 2px !important;
		margin: 0 !important;
	}
}
</style>
