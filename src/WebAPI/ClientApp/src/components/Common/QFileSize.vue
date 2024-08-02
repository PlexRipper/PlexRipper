<template>
	<span>{{ formattedString }}{{ speed ? t('general.units.per-second') : '' }}</span>
</template>

<script setup lang="ts">
import prettyBytes from 'pretty-bytes';
import { useLocalizationStore } from '~/store';

const { t } = useI18n();
const localizationStore = useLocalizationStore();

const props = defineProps<{
	size: number;
	speed?: boolean;
}>();

const formattedString = computed(() => {
	return prettyBytes(props.size, { locale: localizationStore.getLanguageLocale.bcp47Code });
});
</script>
