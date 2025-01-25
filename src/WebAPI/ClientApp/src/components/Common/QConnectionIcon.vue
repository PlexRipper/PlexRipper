<template>
	<q-icon
		:name="icon"
		style="font-size: 2em">
		<q-tooltip
			anchor="top middle"
			self="center middle">
			<QText :value="tooltip" />
		</q-tooltip>
	</q-icon>
</template>

<script setup lang="ts">
import { PlexConnectionTypes } from '@dto';

const { t } = useI18n();
const props = defineProps<{
	type?: PlexConnectionTypes;
}>();

const icon = computed(() => {
	switch (props.type) {
		case PlexConnectionTypes.Local:
			return 'mdi-lan-connect';
		case PlexConnectionTypes.Public:
			return 'mdi-earth';
		case PlexConnectionTypes.PlexRelay:
			return 'mdi-plex';
		default:
			return 'mdi-unknown';
	}
});

const tooltip = computed(() => {
	switch (props.type) {
		case PlexConnectionTypes.Local:
			return t('components.q-connection-icon.tooltip.local-connection');
		case PlexConnectionTypes.Public:
			return t('components.q-connection-icon.tooltip.public-connection');
		case PlexConnectionTypes.PlexRelay:
			return t('components.q-connection-icon.tooltip.plex-connection');
		default:
			return t('general.commands.unknown');
	}
});
</script>
