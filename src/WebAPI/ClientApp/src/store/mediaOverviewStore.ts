import { cloneDeep, isEqual, isNumber, orderBy, sortBy, uniqueId } from 'lodash-es';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { get } from '@vueuse/core';
import {
	type PlexMediaMetadataDTO,
	type PlexMediaSlimDTO,
	type PlexMediaStatisticsDTO,
	PlexMediaType,
	VideoQuality,
	ViewMode,
} from '@dto';
import type { IMediaOverviewSort } from '@composables/event-bus';
import type { IMetaDataMediaFilter, ISelection } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';
import { map, tap } from 'rxjs/operators';
import { defer, forkJoin, iif, type Observable, of } from 'rxjs';
import { useLibraryStore, useSettingsStore } from '@store';
import { getVideoQualityColor, translateVideoQuality } from '@composables';

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
			quality: VideoQuality.None,
		},
		metadataList: {
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
		},
	};

	const state = reactive<IMediaOverviewStoreState>(cloneDeep(defaultState));

	const settingsStore = useSettingsStore();
	const libraryStore = useLibraryStore();

	const actions = {
		refreshMetaData() {
			return plexLibraryApi.getLibraryMediaMetadata(state.libraryId, { mediaType: state.mediaType }).pipe(tap((result) => {
				if (result.isSuccess && result.value) {
					return state.metadataList = result.value;
				}
			}));
		},
		refreshAllLibraryMediaByType(page: number = 0, size: number = 100): Observable<PlexMediaStatisticsDTO | null> {
			return plexMediaApi.getAllMediaByTypeEndpoint({
				mediaType: state.mediaType,
				page,
				size,
				filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
				filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
				...state.metadata,
			}).pipe(map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => {
				if (isSuccess && value) {
					return value;
				}
				return null;
			}));
		},
		refreshLibraryMedia(page: number = 0, size: number = 100): Observable<PlexMediaStatisticsDTO | null> {
			return plexLibraryApi.getPlexLibraryMediaEndpoint(state.libraryId, {
				page,
				size,
				filterOfflineMedia: false,
				filterOwnedMedia: false,
				...state.metadata,
			}).pipe(map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => {
				if (isSuccess && value) {
					return value;
				}
				return null;
			}));
		},
		requestMedia(): Observable<PlexMediaStatisticsDTO | null> {
			if (state.loading) {
				return of(null);
			}

			const page = 0;
			const size = 0;

			state.loading = true;

			return forkJoin([
				actions.refreshMetaData(),
				iif(
					() => state.libraryId === 0,
					// Using defer to prevent both api calls from being executed
					defer(() => actions.refreshAllLibraryMediaByType(page, size)),
					defer(() => actions.refreshLibraryMedia(page, size)))
					.pipe(tap((data) => {
						actions.setMedia(data, state.mediaType);
						state.loading = false;
					}))])
				.pipe(map(([_, media]) => media));
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
			quality,
		}: Partial<IMetaDataMediaFilter>): Observable<PlexMediaStatisticsDTO | null> {
			if (isNumber(countryId)) {
				state.metadata.countryId = countryId;
			}

			if (isNumber(roleId)) {
				state.metadata.roleId = roleId;
			}

			if (isNumber(genreId)) {
				state.metadata.genreId = genreId;
			}

			if (quality) {
				state.metadata.quality = quality;
			}

			return actions.requestMedia();
		},
		unsetMetaData(key: keyof IMetaDataMediaFilter): Observable<PlexMediaStatisticsDTO | null> {
			switch (key) {
				case 'countryId':
				case 'roleId':
				case 'genreId':
					state.metadata[key] = 0;
					break;
				case 'quality':
					state.metadata[key] = VideoQuality.None;
					break;
			}

			return actions.requestMedia();
		},
		clearMetaDataFilter() {
			state.metadata = {
				countryId: 0,
				roleId: 0,
				genreId: 0,
				quality: VideoQuality.None,
			};
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
		getQualities: computed(() => state.metadataList.qualities),
		getFilterChips: computed(() => {
			const result: { text: string; key: keyof IMetaDataMediaFilter; color?: string; id: string }[] = [];

			if (state.metadata.countryId > 0) {
				result.push({
					text: state.metadataList.countries.find((x) => x.id === state.metadata.countryId)?.name ?? '',
					key: 'countryId',
					id: uniqueId(),
				});
			}

			if (state.metadata.roleId > 0) {
				result.push({
					text: state.metadataList.roles.find((x) => x.id === state.metadata.roleId)?.name ?? '',
					key: 'roleId',
					id: uniqueId(),
				});
			}

			if (state.metadata.genreId > 0) {
				result.push({
					text: state.metadataList.genres.find((x) => x.id === state.metadata.genreId)?.name ?? '',
					key: 'genreId',
					id: uniqueId(),
				});
			}

			if (state.metadata.quality != VideoQuality.None) {
				const quality = state.metadataList.qualities.find((x) => x.quality === state.metadata.quality)?.quality;
				result.push({
					text: translateVideoQuality(quality),
					key: 'quality',
					color: getVideoQualityColor(quality),
					id: uniqueId(),
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
