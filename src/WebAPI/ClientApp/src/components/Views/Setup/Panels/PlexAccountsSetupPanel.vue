<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			:header="$t('pages.setup.accounts.title')"
			align="center">
			<AccountOverview />
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
defineProps<{
	name: number;
}>();
</script>
