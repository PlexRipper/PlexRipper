<template>
	<QText
		:cy="cy"
		:value="`${formattedString}${speed ? $t('general.units.per-second') : ''}`" />
</template>

<script setup lang="ts">
import prettyBytes from 'pretty-bytes';
import { useLocalizationStore } from '@store';

const localizationStore = useLocalizationStore();

const props = defineProps<{
	size: number;
	speed?: boolean;
	cy?: string;
}>();

const formattedString = computed(() => {
	return prettyBytes(props.size, { locale: localizationStore.getLanguageLocale?.bcp47Code });
});
</script>
