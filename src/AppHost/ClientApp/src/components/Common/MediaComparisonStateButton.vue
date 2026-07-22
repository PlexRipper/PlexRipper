<template>
	<span
		class="media-comparison-state-button"
		:class="{
			'media-comparison-state-button--with-label': showLabel,
			'media-comparison-state-button--dense': dense,
			'media-comparison-state-button--clickable': clickable,
			[`media-comparison-state-button--${comparisonBadge.tone}`]: true,
		}">
		<BaseButton
			:icon="comparisonBadge.icon"
			:label="resolvedLabel"
			:size="size"
			:flat="flat"
			:round="round"
			:rounded="rounded"
			:outline="outline"
			:tooltip-text="showTooltip ? comparisonBadge.label : ''"
			:cy="cy"
			@click="onClick" />
	</span>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { PlexMediaComparisonState } from '@dto';

type ComparisonBadgeTone
	= 'not-compared'
		| 'owned'
		| 'missing'
		| 'higher-quality'
		| 'pending'
		| 'partial'
		| 'partial-and-higher-quality'
		| 'unknown';

interface IComparisonStateBadge {
	icon: string;
	label: string;
	tone: ComparisonBadgeTone;
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
	clickable?: boolean;
	cy?: string;
}>(), {
	showTooltip: false,
	showLabel: false,
	size: '1rem',
	dense: false,
	flat: true,
	round: false,
	rounded: true,
	outline: false,
	clickable: false,
	cy: undefined,
});

const emit = defineEmits<{
	(e: 'click'): void;
}>();

const { t } = useI18n();

const comparisonBadge = computed((): IComparisonStateBadge => {
	switch (props.comparisonState) {
		case PlexMediaComparisonState.NotCompared:
		case 0:
			return {
				label: t('components.media-overview.comparison.comparison-not-compared'),
				icon: 'mdi-alert-circle-outline',
				tone: 'not-compared',
			};
		case PlexMediaComparisonState.Owned:
		case 1:
			return {
				label: t('components.media-overview.comparison.comparison-owned'),
				icon: 'mdi-check-circle-outline',
				tone: 'owned',
			};
		case PlexMediaComparisonState.Missing:
		case 2:
			return {
				label: t('components.media-overview.comparison.comparison-missing'),
				icon: 'mdi-cloud-download-outline',
				tone: 'missing',
			};
		case PlexMediaComparisonState.HigherQuality:
		case 3:
			return {
				label: t('components.media-overview.comparison.comparison-higher-quality'),
				icon: 'mdi-arrow-up-circle',
				tone: 'higher-quality',
			};
		case PlexMediaComparisonState.Pending:
		case 4:
			return {
				label: t('components.media-overview.comparison.comparison-pending'),
				icon: 'mdi-loading',
				tone: 'pending',
			};
		case PlexMediaComparisonState.Partial:
		case 5:
			return {
				label: t('components.media-overview.comparison.comparison-partial'),
				icon: 'mdi-circle-slice-4',
				tone: 'partial',
			};
		case PlexMediaComparisonState.PartialAndHigherQuality:
		case 6:
			return {
				label: t('components.media-overview.comparison.comparison-partial-and-higher-quality'),
				icon: 'mdi-layers-triple-outline',
				tone: 'partial-and-higher-quality',
			};
		default:
			return {
				label: t('components.media-overview.comparison.unknown'),
				icon: 'mdi-crosshairs-question',
				tone: 'unknown',
			};
	}
});

const resolvedLabel = computed(() => props.showLabel ? get(comparisonBadge).label : undefined);

function onClick() {
	if (props.clickable)
		emit('click');
}
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.media-comparison-state-button {
  white-space: nowrap;
  color: #cfd8dc;

	.q-btn {
    color: currentColor !important;
    background: rgba(10, 14, 22, 0.88);
    border: none !important;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.55);
    backdrop-filter: blur(4px);
    text-transform: none;
    cursor: default;

    .q-icon,
    .block {
      color: currentColor !important;
    }
	}
}

.media-comparison-state-button--clickable {
  .q-btn {
    cursor: pointer;
  }
}

.media-comparison-state-button--owned {
  color: #7fc9a0;
}

.media-comparison-state-button--missing {
  color: #ff9f43;
}

.media-comparison-state-button--higher-quality {
  color: #8ea7ff;
}

.media-comparison-state-button--pending {
  color: #90caf9;
}

.media-comparison-state-button--partial {
  color: #ffc857;
}

.media-comparison-state-button--partial-and-higher-quality {
  color: #d8a7ff;
}

.media-comparison-state-button--unknown {
  color: #ff8a80;
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
