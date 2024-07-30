<template>
	<QPage>
		<QRow>
			<QCol>
				<h1>{{ t('pages.music._musicId.header') }}</h1>
			</QCol>
		</QRow>
	</QPage>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>
