<template>
	<q-tab-panel
		:name="name"
		:data-cy="`setup-panel-${name}`">
		<QSection
			:header="$t('pages.setup.accounts.title')"
			align="center">
			<div class="q-pa-md">
				<AccountOverview />
			</div>
		</QSection>
	</q-tab-panel>
</template>

<script setup lang="ts">
defineProps<{
	name: number;
}>();
</script>
