<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';

interface AccountValidationBtnProps {
	width?: number;
	icon?: string;
	cy?: string;
	textId?: string;
	disabled?: boolean;
	loading?: boolean;
	outline?: boolean | undefined;
	glossy?: boolean | undefined;

	color?: string;
}

const props = withDefaults(defineProps<AccountValidationBtnProps>(), {
	width: 50,
	outline: true,
	glossy: true,
});
</script>
