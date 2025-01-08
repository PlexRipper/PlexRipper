<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			align="center"
			:header="$t('pages.setup.paths.title')">
			<FolderPathsOverview only-defaults />
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
defineProps<{
	name: number;
}>();
</script>
