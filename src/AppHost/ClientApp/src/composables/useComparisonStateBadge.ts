import { PlexMediaComparisonState } from '@dto';

type ComparisonBadgeColor = 'default' | 'positive' | 'warning' | 'negative';

export interface IComparisonStateBadge {
	icon: string;
	label: string;
	color: ComparisonBadgeColor;
}

export function useComparisonStateBadge() {
	const { t } = useI18n();

	function getComparisonStateBadge(comparisonState: PlexMediaComparisonState | number): IComparisonStateBadge {
		switch (comparisonState) {
			case PlexMediaComparisonState.NotCompared:
			case 0:
				return {
					color: 'default',
					label: t('components.media-overview.comparison.comparison-not-compared'),
					icon: 'mdi-alert-circle-outline',
				};
			case PlexMediaComparisonState.Owned:
			case 1:
				return {
					color: 'default',
					label: t('components.media-overview.comparison.comparison-owned'),
					icon: 'mdi-check',
				};
			case PlexMediaComparisonState.Missing:
			case 2:
				return {
					color: 'negative',
					label: t('components.media-overview.comparison.comparison-missing'),
					icon: 'mdi-call-missed',
				};
			case PlexMediaComparisonState.HigherQuality:
			case 3:
				return {
					color: 'positive',
					label: t('components.media-overview.comparison.comparison-higher-quality'),
					icon: 'mdi-arrow-up-circle',
				};
			case PlexMediaComparisonState.Pending:
			case 4:
				return {
					color: 'positive',
					label: t('components.media-overview.comparison.comparison-pending'),
					icon: 'mdi-clock-fast',
				};
			default:
				return {
					color: 'negative',
					label: t('components.media-overview.comparison.unknown'),
					icon: 'mdi-crosshairs-question',
				};
		}
	}

	return { getComparisonStateBadge };
}
