<template>
	<v-row justify="center" class="v-tree-view-table" no-gutters>
		<v-col>
			<!-- Table Headers -->
			<v-row class="v-tree-view-table-header" justify="space-between" no-gutters align="center">
				<v-col>
					<v-row no-gutters class="no-wrap" align="center">
						<!-- Checkbox -->
						<v-col class="select-all-check" style="max-width: 50px" cols="auto">
							<v-checkbox :indeterminate="isIndeterminate" color="red" @change="selectAll($event)"></v-checkbox>
						</v-col>
						<!-- Title -->
						<v-col class="title-column">
							<span>
								{{ headers[0].text }}
							</span>
						</v-col>
					</v-row>
				</v-col>
				<v-col cols="auto" style="margin-right: 6px">
					<v-row no-gutters class="no-wrap" align="center">
						<!-- Other columns -->
						<v-col v-for="(header, i) in headers.slice(1, headers.length)" :key="i" cols="auto">
							<v-sheet :width="header.width" :max-width="header.maxWidth > 0 ? header.maxWidth : 500" class="no-background">
								{{ header.text }}
							</v-sheet>
						</v-col>
					</v-row>
				</v-col>
			</v-row>
			<!-- TreeView Table -->
			<perfect-scrollbar ref="scrollbarmediatable" :options="{ suppressScrollX: true }">
				<v-row no-gutters class="v-tree-view-table-body">
					<v-col class="col px-0">
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
									:open-all="openAll"
									:load-children="getChildren"
									transition
									item-key="key"
									item-text="title"
									class="v-tree-view-table-row"
									@input="updateSelected(i, $event)"
								>
									<template #label="{ item }">
										<v-row align="center">
											<!-- Title -->
											<v-col class="title-column">
												<media-type-icon v-if="mediaIcons" :media-type="item.mediaType" />
												<span class="mt-2">
													{{ item[headers[0].value] }}
												</span>
											</v-col>
										</v-row>
									</template>
									<template #append="{ item }">
										<v-row class="no-wrap" no-gutters align="center">
											<!-- Other columns -->
											<v-col
												v-for="(header, index) in headers.slice(1, headers.length)"
												:key="index"
												:style="{ width: header.width + 'px' }"
												cols="auto"
											>
												<!-- Date format -->
												<template v-if="header.type === 'date'">
													<date-time :text="item[header.value]" :time="false" short-date />
												</template>
												<!-- Date format -->
												<template v-else-if="header.type === 'duration'">
													<duration :value="item[header.value]" />
												</template>
												<!-- Filesize -->
												<template v-else-if="header.type === 'file-size'">
													<file-size :size="item[header.value]" />
												</template>
												<!-- File speed -->
												<template v-else-if="header.type === 'file-speed'">
													<file-size :size="item[header.value]" speed />
												</template>
												<!-- Percentage -->
												<template v-else-if="header.type === 'percentage'">
													<v-progress-linear :value="item.percentage" stream striped color="red" height="25">
														<template #default="{ value }">
															<strong>{{ value }}%</strong>
														</template>
													</v-progress-linear>
												</template>
												<!-- Actions -->
												<template v-else-if="header.type === 'actions'">
													<!-- Default Actions -->
													<template v-if="header.defaultActions && header.defaultActions.length > 0">
														<v-btn
															v-for="(action, y) in header.defaultActions"
															:key="`${index}-${y}`"
															icon
															@click="buttonAction(action, item)"
														>
															<v-icon>{{ buttonIcon(action) }} </v-icon>
														</v-btn>
													</template>

													<!-- Item Actions -->
													<v-btn
														v-for="(action, y) in item[header.value]"
														:key="`${index}-${y}`"
														icon
														@click="buttonAction(action, item)"
													>
														<v-icon>{{ buttonIcon(action) }} </v-icon>
													</v-btn>
												</template>
												<!-- default -->
												<template v-else>
													<span>{{ item[header.value] }}</span>
												</template>
											</v-col>
										</v-row>
									</template>
								</v-treeview>
							</v-lazy>
						</template>
					</v-col>
				</v-row>
			</perfect-scrollbar>
		</v-col>
		<alphabet-navigation v-if="navigation" :items="items" container-ref="scrollbarmediatable" />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import ITreeViewTableHeader from '@components/General/VTreeViewTable/ITreeViewTableHeader';
import ITreeViewTableRow from '@vTreeViewTable/ITreeViewTableRow';
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import ButtonType from '@enums/buttonType';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';

declare interface ISelection {
	index: number;
	keys: string[];
}

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
		AlphabetNavigation,
	},
})
export default class VTreeViewTable extends Vue {
	@Prop({ required: true, type: Array as () => any[] })
	readonly items!: any[];

	@Prop({ required: true, type: Array as () => ITreeViewTableHeader[] })
	readonly headers!: ITreeViewTableHeader[];

	@Prop({ required: false, type: Boolean })
	readonly openAll!: boolean;

	@Prop({ required: false, type: Boolean })
	readonly navigation!: boolean;

	@Prop({ required: false, type: Boolean })
	readonly mediaIcons!: boolean;

	@Prop({ required: false, type: Boolean })
	readonly loadChildren!: boolean;

	selected: ISelection[] = [];
	expanded: string[] = [];
	visible: boolean[] = [];
	loadingButtons: string[] = [];

	@Watch('items')
	updateVisible(): void {
		this.items.forEach(() => this.visible.push(false));
	}

	get getSelected(): string[] {
		return this.selected.map((x) => x.keys).flat(1);
	}

	get isIndeterminate(): boolean {
		return this.getSelected.length !== this.selected.length && this.selected.length > 0;
	}

	get containerRef(): any {
		return this.$refs.scrollbar;
	}

	retrieveAllLeafs(items: ITreeViewTableRow[]): string[] {
		return (
			items
				.map((root) => root.children?.map((child1) => child1.children?.map((child2) => child2.key) ?? child1.key) ?? root.key)
				?.flat(2) ?? []
		);
	}

	isLoading(key: string): boolean {
		return this.loadingButtons.some((x) => x === key);
	}

	updateSelected(i: number, selected: string[]) {
		const index = this.selected.findIndex((x) => x.index === i);
		if (index === -1) {
			this.selected.push({ index: i, keys: selected });
		} else {
			this.selected.splice(index, 1, { index: i, keys: selected });
		}
		this.emitSelected();
	}

	selectAll(state: boolean): void {
		if (state) {
			// this.items.forEach((x, i) => {
			// 	// this.selected.slice(i, 1, { index: i, keys: this.retrieveAllLeafs([x]) } as ISelection);
			// });
		} else {
			this.selected = [];
		}
		this.emitSelected();
	}

	emitSelected(): void {
		this.$emit('selected', this.getSelected);
	}

	buttonAction(action: string, item: any) {
		this.$emit('action', { action, item });
		this.$emit(action, item);
	}

	buttonIcon(buttonType: ButtonType) {
		return Convert.buttonTypeToIcon(buttonType);
	}

	getChildren(item: any): Promise<any> {
		if (this.loadChildren) {
			return new Promise((resolve) => this.$emit('load-children', { item, resolve }));
		}
		return Promise.resolve();
	}
}
</script>
