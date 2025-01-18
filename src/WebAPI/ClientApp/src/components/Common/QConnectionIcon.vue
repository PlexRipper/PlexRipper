<template>
	<q-icon
		:name="local ? 'mdi-lan-connect' : 'mdi-earth'"
		style="font-size: 2em">
		<q-tooltip
			anchor="top middle"
			self="center middle">
			<QText :value="local ? $t('general.tooltip.local-connection') : $t('general.tooltip.public-connection') " />
		</q-tooltip>
	</q-icon>
</template>

<script setup lang="ts">
defineProps<{
	local: boolean;
}>();
</script>
