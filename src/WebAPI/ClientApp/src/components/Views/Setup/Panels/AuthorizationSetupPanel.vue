<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			:header="$t('pages.setup.authorization.title')"
			align="center">
			<AuthenticationOverview />
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
import type { SetupPanelType } from '@enums';

defineProps<{
	name: SetupPanelType;
}>();
</script>
