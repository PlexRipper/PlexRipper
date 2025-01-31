<template>
	<QIconTooltip
		:value="String(type)"
		:options="options" />
</template>

<script setup lang="ts">
import { PlexConnectionTypes } from '@dto';
import type { QIconTooltipData } from '@interfaces';

const { t } = useI18n();
defineProps<{
	type?: PlexConnectionTypes;
}>();

const options = computed((): QIconTooltipData[] => {
	return [{
		value: PlexConnectionTypes.Local,
		icon: 'mdi-lan-connect',
		tooltip: t('components.q-connection-icon.tooltip.local-connection'),
	}, {
		value: PlexConnectionTypes.Public,
		icon: 'mdi-earth',
		tooltip: t('components.q-connection-icon.tooltip.public-connection'),
	}, {
		value: PlexConnectionTypes.PlexRelay,
		icon: 'mdi-plex',
		tooltip: t('components.q-connection-icon.tooltip.plex-connection'),
	}];
});
</script>
