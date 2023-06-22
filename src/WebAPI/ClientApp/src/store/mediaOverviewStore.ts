import { isEqual, orderBy } from 'lodash-es';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import { IMediaOverviewSort } from '@composables/event-bus';

export const useMediaOverviewStore = defineStore('mediaOverviewStore', {
	state: (): {
		items: PlexMediaSlimDTO[];
		itemsLength: number;
		sortedState: IMediaOverviewSort[];
		scrollDict: Record<string, number>;
		scrollAlphabet: string[];
	} => ({
		items: [],
		itemsLength: 0,
		sortedState: [],
		scrollDict: { '#': 0 },
		scrollAlphabet: [],
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
	getters: {},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
