<template>
	<QSection>
		<template #header>
			{{ t('pages.settings.advanced.setup.header') }}
		</template>
		<!--	Reset Database	-->
		<HelpRow help-id="help.settings.advanced.setup-section.go-to-setup-page">
			<GoToButton
				:width="400"
				text-id="go-to-setup-page"
				to="/setup"
				block
			/>
		</HelpRow>
	</QSection>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>
