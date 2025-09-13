<template>
	<QSection :header="t('pages.debug.buttons.alert-display-header')">
		<QAlert type="info">
			{{ t('components.alert-display-debug.info-example') }}
		</QAlert>
		<QAlert type="warning">
			{{ t('components.alert-display-debug.warning-example') }}
		</QAlert>
		<QAlert type="error">
			{{ t('components.alert-display-debug.error-example') }}
		</QAlert>
	</QSection>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>
