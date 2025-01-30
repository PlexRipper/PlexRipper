<template>
	<q-icon
		:name="data.icon"
		style="font-size: 2em">
		<q-tooltip
			anchor="top middle"
			self="center middle">
			<QText :value="data.tooltip" />
		</q-tooltip>
	</q-icon>
</template>

<script setup lang="ts">
import type { QIconTooltipData } from '@interfaces';

const props = withDefaults(defineProps<{
	value: number | string;
	options: QIconTooltipData[]; }>(), {
	value: '',
	options: () => [],
});

const { t } = useI18n();

const data = computed((): QIconTooltipData => {
	const result = props.options.find((x) => x.value === props.value);
	if (!result) {
		return {
			tooltip: t('general.commands.unknown'),
			icon: 'mdi-unknown',
			value: '',
		};
	}
	return result;
});
</script>
