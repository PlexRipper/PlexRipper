import { PlexMediaComparisonState, type PlexMediaSlimDTO } from '@dto';

const PlexMediaComparisonStateByValue = new Map<number, PlexMediaComparisonState>([
	[-1, PlexMediaComparisonState.Unknown],
	[0, PlexMediaComparisonState.NotCompared],
	[1, PlexMediaComparisonState.Owned],
	[2, PlexMediaComparisonState.Pending],
	[3, PlexMediaComparisonState.Missing],
	[4, PlexMediaComparisonState.HigherQuality],
	[5, PlexMediaComparisonState.Partial],
	[6, PlexMediaComparisonState.PartialAndHigherQuality],
]);

export function getPlexMediaComparisonStateFromId(value: number): PlexMediaComparisonState {
	return PlexMediaComparisonStateByValue.get(value) ?? PlexMediaComparisonState.Unknown;
}

export function getPlexMediaComparisonStateId(
	state: PlexMediaComparisonState,
): number {
	return [...PlexMediaComparisonStateByValue].find(([, v]) => v === state)?.[0] ?? -1;
}

export function getPlexMediaComparisonState(mediaItem: PlexMediaSlimDTO): PlexMediaComparisonState {
	return getPlexMediaComparisonStateFromId(mediaItem.comparisonId);
}

export function translateMediaComparisonState(state: PlexMediaComparisonState): string {
	const { $i18n } = useNuxtApp();
	const { t } = $i18n;

	switch (state) {
		case PlexMediaComparisonState.NotCompared:
			return t('components.media-overview.comparison.comparison-not-compared');

		case PlexMediaComparisonState.Owned:
			return t('components.media-overview.comparison.comparison-owned');

		case PlexMediaComparisonState.Missing:
			return t('components.media-overview.comparison.comparison-missing');

		case PlexMediaComparisonState.HigherQuality:
			return t('components.media-overview.comparison.comparison-higher-quality');

		case PlexMediaComparisonState.Pending:
			return t('components.media-overview.comparison.comparison-pending');

		case PlexMediaComparisonState.Partial:
			return t('components.media-overview.comparison.comparison-partial');

		case PlexMediaComparisonState.PartialAndHigherQuality:
			return t('components.media-overview.comparison.comparison-partial-and-higher-quality');

		case PlexMediaComparisonState.Unknown:
		default:
			return t('components.media-overview.comparison.unknown');
	}
}
