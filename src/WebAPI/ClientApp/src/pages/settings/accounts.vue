<template>
	<QPage>
		<QSection>
			<template #header>
				{{ t('pages.settings.accounts.header') }}
			</template>
			<AccountOverview />
		</QSection>
	</QPage>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>
