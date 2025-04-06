import { cloneDeep, isEqual, orderBy, sortBy, isNumber } from 'lodash-es';
import { defineStore, acceptHMRUpdate } from 'pinia';
import { get } from '@vueuse/core';
import {
	PlexMediaType,
	ViewMode,
	type PlexMediaMetadataDTO,
	type PlexMediaSlimDTO,
	type PlexMediaStatisticsDTO,
} from '@dto';
import type { IMediaOverviewSort } from '@composables/event-bus';
import type { IMetaDataMediaFilter, ISelection } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';
import { map, tap } from 'rxjs/operators';
import { iif, defer, type Observable, of, forkJoin } from 'rxjs';
import { useSettingsStore, useLibraryStore } from '@store';

interface IMediaOverviewStoreState {
	libraryId: number;
	items: Readonly<PlexMediaSlimDTO[]>;
	sortedItems: Readonly<PlexMediaSlimDTO[]>;
	itemsLength: number;
	sortedState: IMediaOverviewSort[];
	scrollDict: Record<string, number>;
	scrollAlphabet: string[];
	selection: ISelection;
	downloadButtonVisible: boolean;
	mediaType: PlexMediaType;
	filterQuery: string;
	lastMediaItemViewed: PlexMediaSlimDTO | null;
	loading: boolean;
	isDetailView: boolean;
	allMovieCount: number;
	allTvShowCount: number;
	allSeasonCount: number;
	allEpisodeCount: number;
	allFileSize: number;
	metadata: IMetaDataMediaFilter;
	metadataList: PlexMediaMetadataDTO;
}

export const useMediaOverviewStore = defineStore('MediaOverviewStore', () => {
	const defaultState: IMediaOverviewStoreState = {
		libraryId: 0,
		items: [],
		sortedItems: [],
		itemsLength: 0,
		sortedState: [],
		scrollDict: { '#': 0 },
		scrollAlphabet: [],
		selection: { keys: [], allSelected: false, indexKey: 0 },
		downloadButtonVisible: false,
		mediaType: PlexMediaType.None,
		filterQuery: '',
		lastMediaItemViewed: null,
		loading: false,
		isDetailView: false,
		allMovieCount: 0,
		allTvShowCount: 0,
		allSeasonCount: 0,
		allEpisodeCount: 0,
		allFileSize: 0,
		metadata: {
			countryId: 0,
			roleId: 0,
			genreId: 0,
		},
		metadataList: {
			countries: [],
			roles: [],
			genres: [],
		},
	};

	const state = reactive<IMediaOverviewStoreState>(cloneDeep(defaultState));

	const settingsStore = useSettingsStore();
	const libraryStore = useLibraryStore();

	const actions = {
		refreshMetaData() {
			console.log(state.mediaType);
			return plexLibraryApi.getLibraryMediaMetadata(state.libraryId, { mediaType: state.mediaType }).pipe(tap((result) => {
				if (result.isSuccess && result.value) {
					return state.metadataList = result.value;
				}
			}));
		},
		requestMedia(): Observable<PlexMediaStatisticsDTO | null> {
			if (state.loading) {
				return of(null);
			}

			const page = 0;
			const size = 0;

			state.loading = true;

			return forkJoin([iif(
				() => state.libraryId === 0,
				// Using defer to prevent both api calls from being executed
				defer(() =>
					plexMediaApi.getAllMediaByTypeEndpoint({
						mediaType: state.mediaType,
						page,
						size,
						filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
						filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
						...state.metadata,
					}),
				),
				defer(() =>
					plexLibraryApi.getPlexLibraryMediaEndpoint(state.libraryId, {
						page,
						size,
						filterOfflineMedia: false,
						filterOwnedMedia: false,
						...state.metadata,
					}),
				)),
			actions.refreshMetaData(),
			]).pipe(
				map(([media, _]) => media),
				map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => {
					if (isSuccess && value) {
						return value;
					}
					return null;
				}),
				tap((data) => {
					actions.setMedia(data, state.mediaType);
					state.loading = false;
				}),
			);
		},
		setMedia(data: PlexMediaStatisticsDTO | null, mediaType: PlexMediaType) {
			if (data) {
				state.items = Object.freeze(data.mediaList);
				state.itemsLength = data.mediaCount;
				state.mediaType = mediaType;

				state.allMovieCount = data.movieCount;
				state.allTvShowCount = data.tvShowCount;
				state.allSeasonCount = data.seasonCount;
				state.allEpisodeCount = data.episodeCount;
				state.allFileSize = data.mediaSize;
			} else {
				state.items = Object.freeze([]);
				state.itemsLength = 0;
				state.mediaType = mediaType;

				state.allMovieCount = 0;
				state.allTvShowCount = 0;
				state.allSeasonCount = 0;
				state.allEpisodeCount = 0;
				state.allFileSize = 0;
			}
			state.filterQuery = '';
		},
		setMetaData({
			countryId,
			roleId,
			genreId,
		}: {
			countryId?: number;
			roleId?: number;
			genreId?: number;
		}) {
			state.metadata.countryId = isNumber(countryId) ? countryId : 0;
			state.metadata.roleId = isNumber(roleId) ? roleId : 0;
			state.metadata.genreId = isNumber(genreId) ? genreId : 0;

			actions.requestMedia().subscribe();
		},
		changeAllMediaOverviewType(mediaType: PlexMediaType) {
			state.mediaType = mediaType;
			settingsStore.displaySettings.allOverviewViewMode = mediaType;
			useSubscription(actions.requestMedia().subscribe());
		},
		setFirstLetterIndex() {
			// Create scroll indexes for each letter
			state.scrollDict = {};
			state.scrollDict['#'] = 0;
			// Check for occurrence of title with alphabetic character
			const sortTitles = get(getters.getMediaItems).map((x) => x.title[0]?.toLowerCase() ?? '#');
			let lastIndex = 0;
			const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.toLowerCase();

			for (const letter of alphabet) {
				lastIndex = sortTitles.findIndex((x, idx) => idx >= lastIndex && x === letter);
				if (lastIndex > -1) {
					state.scrollDict[letter] = lastIndex;
				}
			}
			state.scrollAlphabet = Object.keys(state.scrollDict);
		},
		setSelection(selection: ISelection) {
			state.selection = selection;
		},
		getMediaIndex(mediaId: number): number {
			return state.items.findIndex((x) => x.id === mediaId);
		},
		setSelectionRange(min: number, max: number) {
			actions.setSelection({
				indexKey: state.selection.indexKey,
				keys: get(getters.getMediaItems)
					.filter((x) => x.sortIndex >= min && x.sortIndex <= max)
					.map((x) => x.id),
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
		clearSort() {
			state.sortedState = [];
			state.sortedItems = [];
		},
		clearFilter() {
			state.filterQuery = '';
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
			state.sortedItems = Object.freeze(
				orderBy(
					state.items, // Items to sort
					lodashFormat.map((x) => x.field), // Sort by field
					lodashFormat.map((x) => x.sort), // Sort by sort, asc or desc
				),
			);
			state.sortedState = newSortedState;
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {
		hasSelectedMedia: computed((): boolean => {
			return state.selection.keys.length > 0;
		}),
		hasNoSearchResults: computed((): boolean => {
			return state.filterQuery != '' && get(getters.getMediaItems).length === 0;
		}),
		allMediaMode: computed(() => state.libraryId === 0),
		library: computed(() => libraryStore.getLibrary(state.libraryId)),
		getMediaItems: computed((): Readonly<PlexMediaSlimDTO[]> => {
			if (!state.items) {
				return [];
			}
			// Currently sorting
			const query = state.filterQuery.toLowerCase();
			if (state.sortedState.length > 0) {
				if (state.filterQuery != '') {
					return state.sortedItems.filter((x) => x.searchTitle.includes(query));
				}
				return state.sortedItems;
			} else {
				if (state.filterQuery != '') {
					return state.items.filter((x) => x.searchTitle.includes(query));
				}
				return state.items;
			}
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
			return get(getters.getMediaViewMode) === ViewMode.Table && !state.isDetailView;
		}),
		showDownloadButton: computed((): boolean => {
			return state.downloadButtonVisible || (get(getters.hasSelectedMedia) && get(getters.getMediaViewMode) === ViewMode.Table);
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
		getGenres: computed(() => sortBy(state.metadataList.genres, (x) => x.name)),
		getRoles: computed(() => sortBy(state.metadataList.roles, (x) => x.name)),
		getCountries: computed(() => sortBy(state.metadataList.countries, (x) => x.name)),
		getFilterChips: computed(() => {
			const result: { text: string; key: keyof IMetaDataMediaFilter }[] = [];

			if (state.metadata.countryId > 0) {
				result.push({
					text: state.metadataList.countries.find((x) => x.id === state.metadata.countryId)?.name ?? '',
					key: 'countryId',
				});
			}

			if (state.metadata.roleId > 0) {
				result.push({
					text: state.metadataList.roles.find((x) => x.id === state.metadata.roleId)?.name ?? '',
					key: 'roleId',
				});
			}

			if (state.metadata.genreId > 0) {
				result.push({
					text: state.metadataList.genres.find((x) => x.id === state.metadata.genreId)?.name ?? '',
					key: 'genreId',
				});
			}

			return result;
		}),
	};

	watch(getters.getMediaItems, () => actions.setFirstLetterIndex());

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
