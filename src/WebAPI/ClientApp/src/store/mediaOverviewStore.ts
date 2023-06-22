import { isEqual, orderBy } from 'lodash-es';
import { acceptHMRUpdate } from 'pinia';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import { IMediaOverviewSort } from '@composables/event-bus';
import ISelection from '@interfaces/ISelection';

export const useMediaOverviewStore = defineStore('mediaOverviewStore', {
	state: (): {
		items: PlexMediaSlimDTO[];
		itemsLength: number;
		sortedState: IMediaOverviewSort[];
		scrollDict: Record<string, number>;
		scrollAlphabet: string[];
		selection: ISelection;
		downloadButtonVisible: boolean;
	} => ({
		items: [],
		itemsLength: 0,
		sortedState: [],
		scrollDict: { '#': 0 },
		scrollAlphabet: [],
		selection: { keys: [], allSelected: false, indexKey: 0 },
		downloadButtonVisible: true,
	}),
	actions: {
		setMedia(items: PlexMediaSlimDTO[]) {
			this.items = items;
			this.itemsLength = items.length;
			// Create scroll indexes for each letter
			this.scrollDict['#'] = 0;
			// Check for occurrence of title with alphabetic character
			const sortTitles = this.items.map((x) => x.sortTitle[0]?.toLowerCase() ?? '#');
			let lastIndex = 0;
			for (const letter of 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.toLowerCase()) {
				const index = sortTitles.indexOf(letter, lastIndex);
				if (index > -1) {
					this.scrollDict[letter] = index;
					lastIndex = index;
				}
			}
			this.scrollAlphabet = Object.keys(this.scrollDict);
		},
		setSelection(selection: ISelection) {
			this.selection = selection;
		},
		setRootSelected(value: boolean) {
			this.setSelection({
				indexKey: this.selection?.indexKey ?? 0,
				keys: value ? this.items.map((x) => x.id) : [],
				allSelected: value,
			} as ISelection);
		},
		sortMedia(event: IMediaOverviewSort) {
			const newSortedState = [...this.sortedState];
			const index = newSortedState.findIndex((x) => x.field === event.field);
			if (index > -1) {
				newSortedState.splice(index, 1);
			}
			if (event.sort) {
				newSortedState.unshift(event);
			}

			// Prevent unnecessary sorting
			if (isEqual(this.sortedState, newSortedState)) {
				return;
			}
			const lodashFormat = newSortedState.map((x) => {
				return {
					field: x.field,
					sort: x.sort !== 'no-sort' ? x.sort : false,
				};
			});
			this.items = orderBy(
				this.items, // Items to sort
				lodashFormat.map((x) => x.field), // Sort by field
				lodashFormat.map((x) => x.sort), // Sort by sort, asc or desc
			);
			this.sortedState = newSortedState;
		},
	},
	getters: {
		hasSelectedMedia(): boolean {
			return this.selection.keys.length > 0;
		},
		isRootSelected(): boolean | null {
			if (this.selection?.keys.length === this.itemsLength) {
				return true;
			}

			if (this.selection?.keys.length === 0) {
				return false;
			}

			return null;
		},
	},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
