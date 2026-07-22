<template>
	<span
		class="media-comparison-state-button"
		:class="{
			'media-comparison-state-button--with-label': showLabel,
			'media-comparison-state-button--dense': dense,
		}">
		<BaseButton
			:color="comparisonBadge.color"
			:icon="comparisonBadge.icon"
			:label="showLabel ? comparisonBadge.label : undefined"
			:size="size"
			:flat="flat"
			:round="round"
			:rounded="rounded"
			:outline="outline"
			:tooltip-text="showTooltip ? comparisonBadge.label : ''"
			:cy="cy" />
	</span>
</template>

<script setup lang="ts">
import { PlexMediaComparisonState } from '@dto';

type ComparisonBadgeColor = 'default' | 'positive' | 'warning' | 'negative';

interface IComparisonStateBadge {
	icon: string;
	label: string;
	color: ComparisonBadgeColor;
}

const props = withDefaults(defineProps<{
	comparisonState: PlexMediaComparisonState | number;
	showTooltip?: boolean;
	showLabel?: boolean;
	size?: string;
	dense?: boolean;
	flat?: boolean;
	round?: boolean;
	rounded?: boolean;
	outline?: boolean;
	cy?: string;
}>(), {
	showTooltip: false,
	showLabel: false,
	size: '1rem',
	dense: false,
	flat: true,
	round: true,
	rounded: false,
	outline: false,
	cy: undefined,
});

const { t } = useI18n();

const comparisonBadge = computed((): IComparisonStateBadge => {
	switch (props.comparisonState) {
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
});
</script>

<style lang="scss">
.media-comparison-state-button {
  white-space: nowrap;

  .q-btn {
    text-transform: none;
  }
}

.media-comparison-state-button--with-label {
  min-width: max-content;
}

.media-comparison-state-button--dense {
  .q-btn {
    min-height: 2em;
    padding: 0 0.5em;
  }
}
</style>
