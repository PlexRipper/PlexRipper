<template>
	<q-page>
		<q-section>
			<template #header>
				{{ t('pages.settings.accounts.header') }}
			</template>
			<account-overview />
		</q-section>
	</q-page>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>
