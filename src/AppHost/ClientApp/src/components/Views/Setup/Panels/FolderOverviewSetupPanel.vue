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
import type { SetupPanelType } from '@enums';

defineProps<{
	name: SetupPanelType;
}>();
</script>
