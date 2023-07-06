<template>
	<span>{{ formattedString }}{{ speed ? t('general.units.per-second') : '' }}</span>
</template>

<script setup lang="ts">
import prettyBytes from 'pretty-bytes';
const { t } = useI18n();
const settingsStore = useSettingsStore();
const props = defineProps<{
	size: number;
	speed?: boolean;
}>();

const formattedString = computed(() => {
	return prettyBytes(props.size, { locale: settingsStore.getLanguageLocale.bcp47Code });
});
</script>
