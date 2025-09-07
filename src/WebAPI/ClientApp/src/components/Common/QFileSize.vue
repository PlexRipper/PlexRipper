<template>
	<QText
		:cy="cy"
		:align="align"
		:value="`${formattedString}${speed ? t('general.units.per-second') : ''}`" />
</template>

<script setup lang="ts">
import prettyBytes from 'pretty-bytes';
import { useLocalizationStore } from '@store';
import type { IQTextProps } from '@interfaces';

const localizationStore = useLocalizationStore();
const { t } = useI18n();

const props = defineProps<Pick<IQTextProps, 'align'> & {
	size: number;
	speed?: boolean;
	cy?: string;
}>();

const formattedString = computed(() => {
	return prettyBytes(props.size, { locale: localizationStore.getLanguageLocale.bcp47Code });
});
</script>
