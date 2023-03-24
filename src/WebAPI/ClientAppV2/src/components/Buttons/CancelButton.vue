<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { defineProps, withDefaults } from 'vue';
import { IBaseButtonProps } from '~/types/props/base-button/IBaseButtonProps';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	icon: 'mdi-alert',
	outline: true,
	cy: '',
	label: 'Warning',
	textId: 'cancel',
});
</script>
