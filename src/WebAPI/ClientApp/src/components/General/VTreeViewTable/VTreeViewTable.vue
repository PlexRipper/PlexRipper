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
			<v-row ref="v-tree-view-container" no-gutters class="v-tree-view-table-body" :style="{ height: getHeight }">
				<perfect-scrollbar ref="scrollbarmediatable" :options="{ suppressScrollX: true }">
					<v-col class="col pa-0">
						<template v-for="(parentItem, i) in items">
							<v-lazy
								:key="i"
								:options="{
									threshold: 0.25,
								}"
								:min-height="50"
								:data-title="parentItem.title[0]"
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
									transition
									:item-key="itemKey"
									item-text="title"
									class="v-tree-view-table-row"
									:value="findSelected(parentItem.key)"
									@input="updateSelected(parentItem.key, $event)"
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
				</perfect-scrollbar>
			</v-row>
		</v-col>
		<alphabet-navigation v-if="navigation" :items="items" container-ref="scrollbarmediatable" />
	</v-row>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import ITreeViewTableHeader from '@components/General/VTreeViewTable/ITreeViewTableHeader';
import ITreeViewTableRow from '@vTreeViewTable/ITreeViewTableRow';
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import ButtonType from '@enums/buttonType';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';

declare interface ISelection {
	indexKey: number;
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

	@Prop({ required: false, type: String, default: 'key' })
	readonly itemKey!: string;

	@Prop({ required: false, type: Boolean, default: false })
	readonly heightAuto!: boolean;

	selected: ISelection[] = [];
	expanded: string[] = [];
	visible: boolean[] = [];
	loadingButtons: string[] = [];
	isMounted: boolean = false;
	@Watch('items')
	updateVisible(): void {
		this.items.forEach(() => this.visible.push(false));
	}

	get getSelected(): string[] {
		return this.selected.map((x) => x.keys).flat(1);
	}

	findSelected(i: number): string[] {
		return this.selected.find((x) => x.indexKey === i)?.keys ?? [];
	}

	get isIndeterminate(): boolean {
		return this.selected.length < this.items.length && this.selected.length > 0;
	}

	get containerRef(): any {
		return this.$refs.scrollbar;
	}

	get getHeight(): string {
		if (this.heightAuto) {
			return 'auto';
		}
		if (this.isMounted) {
			const height = this.$vuetify.breakpoint.height - this.$refs['v-tree-view-container']?.getBoundingClientRect().top ?? 0;
			Log.debug('v-tree-view-container height: ', height);
			return height + 'px';
		}
		return 'auto';
	}

	hasLoadableChildren(item: ITreeViewTableRow): boolean {
		return item?.children?.length === 0 ?? false;
	}

	retrieveAllLeafs(items: ITreeViewTableRow[]): string[] {
		const keys: string[] = [];
		items.forEach((root) => {
			if (root.children && root.children?.length > 0) {
				root.children.forEach((child1) => {
					if (child1.children && child1.children.length > 0) {
						child1.children.forEach((child2) => {
							if (child2.children && child2.children.length > 0) {
								child2.children.forEach((child3) => {
									keys.push(child3[this.itemKey]);
								});
							} else {
								keys.push(child2[this.itemKey]);
							}
						});
					} else {
						keys.push(child1[this.itemKey]);
					}
				});
			} else {
				keys.push(root[this.itemKey]);
			}
		});

		return keys;
	}

	isLoading(key: string): boolean {
		return this.loadingButtons.some((x) => x === key);
	}

	updateSelected(itemKey: number, selected: string[]) {
		const index = this.selected.findIndex((x) => x.indexKey === itemKey);
		if (index === -1) {
			this.selected.push({ indexKey: itemKey, keys: selected });
		} else {
			this.selected.splice(index, 1, { indexKey: itemKey, keys: selected });
		}
		this.emitSelected();
	}

	selectAll(state: boolean): void {
		if (state) {
			this.items.forEach((item) => {
				const leafs = this.retrieveAllLeafs([item]);
				this.updateSelected(item.key, leafs);
			});
		} else {
			this.selected = [];
			this.emitSelected();
		}
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
			const promise = new Promise((resolve) => this.$emit('load-children', { item, resolve }));
			// Ensure that all children are selected if the root node is selected on load-children
			promise.then(() => {
				const selection = this.selected.find((x) => x.indexKey === item.key);
				if (selection) {
					const leafs = this.retrieveAllLeafs([item]);
					this.updateSelected(item.key, leafs);
				}
			});
			return promise;
		}
		return Promise.resolve();
	}

	mounted(): void {
		this.$nextTick(function () {
			this.isMounted = true;
		});
	}
}
</script>
