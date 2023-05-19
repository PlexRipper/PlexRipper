<template>
	<q-page>
		<q-row>
			<q-col>
				<h1>{{ t('pages.music._musicId.header') }}</h1>
			</q-col>
		</q-row>
	</q-page>
</template>
<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>
