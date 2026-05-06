import Log from 'consola';
import { cloneDeep, isNumber, sortBy, uniqueId } from 'lodash-es';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { computed, reactive, toRefs } from 'vue';
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
import { MediaSortField, SortDirection } from '@enums';
import { type IMetaDataMediaFilter, type ISelection, type ISortOption, StoreNames } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';
import { map, tap, takeUntil } from 'rxjs/operators';
import { defer, forkJoin, type Observable, of, Subject } from 'rxjs';
import { useLibraryStore, useSettingsStore } from '@store';
import { getVideoQualityColor, translateVideoQuality } from '@composables';
import { useSubscription } from '@vueuse/rxjs';
import { useI18n } from 'vue-i18n';

interface IMediaOverviewStoreState {
	libraryId: number;
	items: Readonly<PlexMediaSlimDTO[]>;
	sortedItems: Readonly<PlexMediaSlimDTO[]>;
	itemsLength: number;
	sortedState: IMediaOverviewSort;
	scrollDict: Map<string, number>;
	selection: ISelection;
	downloadButtonVisible: boolean;
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
	pageSize: number;
	totalCount: number;
	pageCache: Map<number, Readonly<PlexMediaSlimDTO[]>>;
	loadedPages: Set<number>;
	loadingPages: Set<number>;
	navigationIndexes: Map<string, number>;
	pendingScrollIndex: number | null;
}

export const useMediaOverviewStore = defineStore(StoreNames.MediaOverviewStore, () => {
	const defaultState: IMediaOverviewStoreState = {
		libraryId: 0,
		items: [],
		sortedItems: [],
		itemsLength: 0,
		sortedState: { field: MediaSortField.Title, sort: SortDirection.Asc },
		scrollDict: new Map<string, number>([['#', 0]]),
		selection: { keys: [], allSelected: false, indexKey: 0 },
		downloadButtonVisible: false,
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
			mediaCount: 0,
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
			navigationIndexes: {},
		},
		pageSize: 100,
		totalCount: 0,
		pageCache: new Map<number, Readonly<PlexMediaSlimDTO[]>>(),
		loadedPages: new Set<number>(),
		loadingPages: new Set<number>(),
		navigationIndexes: new Map<string, number>(),
		pendingScrollIndex: null,
	};

	const state = reactive<IMediaOverviewStoreState>(cloneDeep(defaultState));
	const settingsStore = useSettingsStore();
	const libraryStore = useLibraryStore();

	// Subject to cancel in-flight requests when switching libraries
	const cancelSubject$ = new Subject<void>();

	const actions = {
		cancelPendingRequests() {
			Log.debug('Cancelling pending media requests');
			cancelSubject$.next();
			state.loading = false;
		},
		initializeLibrary(libraryId: number): Observable<PlexMediaStatisticsDTO | null> {
			actions.cancelPendingRequests();
			state.libraryId = libraryId;
			state.isDetailView = false;
			actions.clearMetaDataFilter();
			actions.clearSort();
			return actions.requestMedia();
		},
		getPageForIndex(index: number): number {
			return Math.max(0, Math.floor(index / state.pageSize));
		},
		resetPagedCache() {
			state.pageCache = new Map<number, Readonly<PlexMediaSlimDTO[]>>();
			state.loadedPages = new Set<number>();
			state.loadingPages = new Set<number>();
			state.totalCount = 0;
			state.navigationIndexes = new Map<string, number>();
			state.scrollDict = new Map<string, number>();
			state.pendingScrollIndex = null;
		},
		setPage(page: number, data: PlexMediaStatisticsDTO | null) {
			if (!data) {
				return;
			}

			state.pageCache.set(page, Object.freeze(data.mediaList));
			state.loadedPages.add(page);
			state.totalCount = data.totalCount;
			state.itemsLength = data.totalCount;

			state.allMovieCount = data.movieCount;
			state.allTvShowCount = data.tvShowCount;
			state.allSeasonCount = data.seasonCount;
			state.allEpisodeCount = data.episodeCount;
			state.allFileSize = data.mediaSize;

			const pages = [...state.pageCache.keys()].sort((a, b) => a - b);
			const items = pages.flatMap((key) => state.pageCache.get(key) ?? []);
			state.items = Object.freeze(items);
			state.sortedItems = Object.freeze(items);
		},
		getItemByIndex(index: number): PlexMediaSlimDTO | null {
			if (index < 0 || index >= state.totalCount) {
				return null;
			}

			const page = actions.getPageForIndex(index);
			const pageItems = state.pageCache.get(page);
			if (!pageItems) {
				return null;
			}

			return pageItems[index - page * state.pageSize] ?? null;
		},
		isIndexLoaded(index: number): boolean {
			return actions.getItemByIndex(index) !== null;
		},
		refreshMetaData() {
			return plexLibraryApi.getLibraryMediaMetadata(state.libraryId, {
				mediaType: get(getters.getMediaType),
				search: state.filterQuery,
				sortField: state.sortedState.field,
				sortDirection: state.sortedState.sort,
				filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
				filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
				...state.metadata,
			}).pipe(
				takeUntil(cancelSubject$),
				tap((result) => {
					if (result.isSuccess && result.value) {
						state.metadataList = result.value;
						state.navigationIndexes = new Map(Object.entries(result.value.navigationIndexes ?? {}));
						state.scrollDict = new Map(Object.entries(result.value.navigationIndexes ?? {}));
					}
				}),
			);
		},
		refreshAllLibraryMediaByType(page: number = 0, size: number = 100): Observable<PlexMediaStatisticsDTO | null> {
			return plexMediaApi.getAllMediaByTypeEndpoint({
				mediaType: get(getters.getMediaType),
				page,
				size,
				search: state.filterQuery,
				sortField: state.sortedState.field,
				sortDirection: state.sortedState.sort,
				filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
				filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
				...state.metadata,
			}).pipe(
				takeUntil(cancelSubject$),
				map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => (isSuccess && value) ? value : null),
			);
		},
		refreshLibraryMedia(page: number = 0, size: number = 100): Observable<PlexMediaStatisticsDTO | null> {
			return plexLibraryApi.getPlexLibraryMediaEndpoint(state.libraryId, {
				page,
				size,
				search: state.filterQuery,
				sortField: state.sortedState.field,
				sortDirection: state.sortedState.sort,
				filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
				filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
				...state.metadata,
			}).pipe(
				takeUntil(cancelSubject$),
				map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => (isSuccess && value) ? value : null),
			);
		},
		requestMediaFirstPage(): Observable<PlexMediaStatisticsDTO | null> {
			if (state.loading) {
				return of(null);
			}

			actions.resetPagedCache();

			const page = 0;
			const size = state.pageSize;

			state.loading = true;

			return forkJoin([
				actions.refreshMetaData(),
				defer(() => state.libraryId > 0 ? libraryStore.refreshLibrary(state.libraryId) : of(null)).pipe(takeUntil(cancelSubject$)),
				defer(() => state.libraryId === 0 ? actions.refreshAllLibraryMediaByType(page, size) : actions.refreshLibraryMedia(page, size)).pipe(
					tap((data) => {
						actions.setPage(page, data);
					}),
				),
			]).pipe(
				takeUntil(cancelSubject$),
				map(([_, __, media]) => media),
				tap({
					next: () => {
						state.loading = false;
					},
					error: (err) => {
						state.loading = false;
						Log.error('Initial paged media request failed', err);
					},
					complete: () => {
						state.loading = false;
					},
				}),
			);
		},
		requestMedia(): Observable<PlexMediaStatisticsDTO | null> {
			return actions.requestMediaFirstPage();
		},
		requestMediaPage(page: number): Observable<PlexMediaStatisticsDTO | null> {
			if (page < 0) {
				return of(null);
			}

			if (state.loadedPages.has(page) || state.loadingPages.has(page)) {
				return of(null);
			}

			state.loadingPages.add(page);

			return defer(() =>
				state.libraryId === 0
					? actions.refreshAllLibraryMediaByType(page, state.pageSize)
					: actions.refreshLibraryMedia(page, state.pageSize),
			).pipe(
				takeUntil(cancelSubject$),
				tap((data) => actions.setPage(page, data)),
				tap({
					next: () => state.loadingPages.delete(page),
					error: (err) => {
						state.loadingPages.delete(page);
						Log.error('Paged media request failed', { page, err });
					},
					complete: () => state.loadingPages.delete(page),
				}),
			);
		},
		ensureRangeLoaded(startIndex: number, endIndex: number): Observable<(PlexMediaStatisticsDTO | null)[]> {
			if (state.totalCount <= 0) {
				return of([]);
			}

			const safeStart = Math.max(0, startIndex);
			const safeEnd = Math.min(state.totalCount - 1, endIndex);

			if (safeEnd < safeStart) {
				return of([]);
			}

			const startPage = actions.getPageForIndex(safeStart);
			const endPage = actions.getPageForIndex(safeEnd);
			const requests: Observable<PlexMediaStatisticsDTO | null>[] = [];

			for (let page = startPage; page <= endPage; page++) {
				if (!state.loadedPages.has(page) && !state.loadingPages.has(page)) {
					requests.push(actions.requestMediaPage(page));
				}
			}

			return requests.length ? forkJoin(requests) : of([]);
		},
		prefetchAroundIndex(index: number, radius: number = 50): Observable<(PlexMediaStatisticsDTO | null)[]> {
			return actions.ensureRangeLoaded(index - radius, index + radius);
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

			return actions.requestMediaFirstPage();
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

			return actions.requestMediaFirstPage();
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
			settingsStore.displaySettings.allOverviewViewMode = mediaType;
			useSubscription(actions.requestMediaFirstPage().subscribe());
		},
		// This function calculates the scroll navigation options based on the current media items and active sorting.
		setMediaIndexNavigationOptions() {
			state.scrollDict = new Map(state.navigationIndexes);
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
			const loadedIds = [...state.pageCache.values()]
				.flatMap((items) => items.map((x) => x.id));

			actions.setSelection({
				indexKey: state.selection?.indexKey ?? 0,
				keys: value ? loadedIds : [],
				allSelected: false,
			} as ISelection);
		},
		clearSort() {
			state.sortedState = { field: MediaSortField.Title, sort: SortDirection.Asc };
			state.sortedItems = [];
		},
		clearFilter() {
			state.filterQuery = '';
			useSubscription(actions.requestMediaFirstPage().subscribe());
		},
		setFilterQuery(value: string) {
			state.filterQuery = value;
		},
		toggleSortMedia(field: MediaSortField) {
			if (state.sortedState.field === field) {
				state.sortedState.sort = state.sortedState.sort === SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;
			} else {
				state.sortedState = { field, sort: SortDirection.Asc };
			}

			useSubscription(actions.requestMediaFirstPage().subscribe());
		},
		sortMedia(event: IMediaOverviewSort) {
			state.sortedState = event;
			useSubscription(actions.requestMediaFirstPage().subscribe());
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
			return state.filterQuery !== '' && state.totalCount === 0;
		}),
		hasNoFilterResults: computed((): boolean => {
			return state.metadataList.mediaCount > 0 && state.totalCount === 0;
		}),
		allMediaMode: computed(() => state.libraryId === 0),
		library: computed(() => libraryStore.getLibrary(state.libraryId)),
		getMediaItems: computed((): Readonly<PlexMediaSlimDTO[]> => {
			return state.items;
		}),
		getMediaViewMode: computed((): ViewMode => {
			switch (get(getters.getMediaType)) {
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
			const loadedIds = [...state.pageCache.values()].flatMap((items) => items.map((x) => x.id));
			if (loadedIds.length > 0 && state.selection?.keys.length === loadedIds.length) {
				return true;
			}

			if (state.selection?.keys.length === 0) {
				return false;
			}

			return null;
		}),
		getActiveSort: computed((): IMediaOverviewSort => {
			return state.sortedState;
		}),
		getMediaType: computed((): PlexMediaType => state.libraryId === 0 ? settingsStore.displaySettings.allOverviewViewMode : libraryStore.getLibrary(state.libraryId)?.type ?? PlexMediaType.None),
		getIsSorted: computed((): boolean => {
			if (state.sortedState.sort === SortDirection.NoSort) {
				return false;
			}
			return !(state.sortedState.field === MediaSortField.Title && state.sortedState.sort === SortDirection.Asc);
		}),
		getSortOptions: (): ISortOption[] => {
			const { t } = useI18n({ useScope: 'global' });
			const options: ISortOption[] = [
				{ field: MediaSortField.Title, label: t('general.sort.title'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Year, label: t('general.sort.year'), direction: SortDirection.NoSort },
				{ field: MediaSortField.AddedAt, label: t('general.sort.added-at'), direction: SortDirection.NoSort },
				{ field: MediaSortField.UpdatedAt, label: t('general.sort.updated-at'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Duration, label: t('general.sort.duration'), direction: SortDirection.NoSort },
				{ field: MediaSortField.MediaSize, label: t('general.sort.size'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Quality, label: t('general.sort.quality'), direction: SortDirection.NoSort },
			];

			for (const option of options) {
				if (option.field === state.sortedState.field) {
					option.direction = state.sortedState.sort;
				}
			}

			return options;
		},
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

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
