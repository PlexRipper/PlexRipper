<template>
	<BaseButton v-bind="props"/>
</template>

<script setup lang="ts">
import {withDefaults, defineProps} from 'vue';
import BaseButton from '@buttons/BaseButton.vue';

interface ConfirmBtnProps {
	disabled?: boolean;
	loading?: boolean;
	icon?: string;
	cy?: string;
	label?: string;
}

const props = withDefaults(defineProps<ConfirmBtnProps>(), {
	cy: '',
	outline: true,
	textId: 'delete',
	icon: 'mdi-delete',
	color: 'red',
	iconOnly: true,
	iconSize: 32,
	height: 50,
});
</script>
