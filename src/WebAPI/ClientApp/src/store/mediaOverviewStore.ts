import { isEqual, orderBy } from 'lodash-es';
import { defineStore, acceptHMRUpdate } from 'pinia';
import { get } from '@vueuse/core';
import { PlexMediaType, ViewMode, type PlexMediaSlimDTO } from '@dto';
import type { IMediaOverviewSort } from '@composables/event-bus';
import type { ISelection } from '@interfaces';
import { useSettingsStore } from '#build/imports';

export const useMediaOverviewStore = defineStore('MediaOverviewStore', () => {
	const state = reactive<{
		items: Readonly<PlexMediaSlimDTO[]>;
		itemsLength: number;
		sortedState: IMediaOverviewSort[];
		scrollDict: Record<string, number>;
		scrollAlphabet: string[];
		selection: ISelection;
		downloadButtonVisible: boolean;
		showMediaOverview: boolean;
		mediaType: PlexMediaType;
	}>({
		items: [],
		itemsLength: 0,
		sortedState: [],
		scrollDict: { '#': 0 },
		scrollAlphabet: [],
		selection: { keys: [], allSelected: false, indexKey: 0 },
		downloadButtonVisible: false,
		showMediaOverview: true,
		mediaType: PlexMediaType.None,
	});

	const settingsStore = useSettingsStore();

	const actions = {
		setMedia(items: PlexMediaSlimDTO[], mediaType: PlexMediaType) {
			state.scrollDict = {};
			state.items = Object.freeze(items);
			state.itemsLength = state.items.length;
			state.mediaType = mediaType;
			// Create scroll indexes for each letter
			state.scrollDict['#'] = 0;
			// Check for occurrence of title with alphabetic character
			const sortTitles = state.items.map((x) => x.sortTitle[0]?.toLowerCase() ?? '#');
			let lastIndex = 0;
			for (const letter of 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.toLowerCase()) {
				const index = sortTitles.indexOf(letter, lastIndex);
				if (index > -1) {
					state.scrollDict[letter] = index;
					lastIndex = index;
				}
			}
			state.scrollAlphabet = Object.keys(state.scrollDict);
		},
		setSelection(selection: ISelection) {
			state.selection = selection;
		},
		setSelectionRange(min: number, max: number) {
			actions.setSelection({
				indexKey: state.selection.indexKey,
				keys: state.items.filter((x) => x.index >= min && x.index <= max).map((x) => x.id),
				allSelected: false,
			} as ISelection);
		},
		setRootSelected(value: boolean) {
			actions.setSelection({
				indexKey: state.selection?.indexKey ?? 0,
				keys: value ? state.items.map((x) => x.id) : [],
				allSelected: value,
			} as ISelection);
		},
		sortMedia(event: IMediaOverviewSort) {
			const newSortedState = [...state.sortedState];
			const index = newSortedState.findIndex((x) => x.field === event.field);
			if (index > -1) {
				newSortedState.splice(index, 1);
			}
			if (event.sort) {
				newSortedState.unshift(event);
			}

			// Prevent unnecessary sorting
			if (isEqual(state.sortedState, newSortedState)) {
				return;
			}
			const lodashFormat = newSortedState.map((x) => {
				return {
					field: x.field,
					sort: x.sort !== 'no-sort' ? x.sort : false,
				};
			});
			state.items = orderBy(
				state.items, // Items to sort
				lodashFormat.map((x) => x.field), // Sort by field
				lodashFormat.map((x) => x.sort), // Sort by sort, asc or desc
			);
			state.sortedState = newSortedState;
		},
	};
	const getters = {
		hasSelectedMedia: computed((): boolean => {
			return state.selection.keys.length > 0;
		}),
		getMediaViewMode: computed((): ViewMode => {
			switch (state.mediaType) {
				case PlexMediaType.Movie:
					return settingsStore.displaySettings.movieViewMode;
				case PlexMediaType.TvShow:
					return settingsStore.displaySettings.tvShowViewMode;
				default:
					return ViewMode.Poster;
			}
		}),
		showSelectionButton: computed((): boolean => {
			return state.showMediaOverview && get(getters.getMediaViewMode) === ViewMode.Table;
		}),
		showDownloadButton: computed((): boolean => {
			return state.downloadButtonVisible || (getters.hasSelectedMedia && get(getters.getMediaViewMode) === ViewMode.Table);
		}),
		isRootSelected: computed((): boolean | null => {
			if (state.selection?.keys.length === state.itemsLength) {
				return true;
			}

			if (state.selection?.keys.length === 0) {
				return false;
			}

			return null;
		}),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
