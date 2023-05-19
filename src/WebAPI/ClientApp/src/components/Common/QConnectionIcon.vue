<template>
	<q-icon :name="local ? 'mdi-lan-connect' : 'mdi-earth'" style="font-size: 2em">
		<q-tooltip anchor="top middle" self="center middle">
			<span>{{ local ? t('general.tooltip.local-connection') : t('general.tooltip.public-connection') }}</span>
		</q-tooltip>
	</q-icon>
</template>

<script setup lang="ts">
const { t } = useI18n();

defineProps<{
	local: boolean;
}>();
</script>
