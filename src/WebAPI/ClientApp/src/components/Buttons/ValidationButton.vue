<template>
	<BaseButton
		:loading="loading"
		:color="validationStyle.color"
		:icon="validationStyle.icon"
		:label="validationStyle.text" />
</template>

<script setup lang="ts">
import { get, set, watchOnce } from '@vueuse/core';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps<{
	loading: boolean;
	isValidated: boolean;
}>();

const isExecuted = ref(false);

watchOnce(() => props.loading, () => set(isExecuted, true));

const validationStyle = computed((): {
	color: 'default' | 'positive' | 'warning' | 'negative';
	icon: string;
	text: string;
} => {
	if (!get(isExecuted) || props.loading) {
		return {
			color: 'default',
			icon: 'mdi-text-box-search-outline',
			text: t('general.commands.validate'),
		};
	}

	if (props.isValidated) {
		return {
			color: 'positive',
			icon: 'mdi-check-bold',
			text: '',
		};
	} else {
		return {
			color: 'negative',
			icon: 'mdi-alert-circle-outline',
			text: t('general.commands.validate'),
		};
	}
});
</script>

<style lang="scss">

</style>
