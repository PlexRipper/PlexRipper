<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import BaseButton from '@buttons/BaseButton.vue';
import { IBaseButtonProps } from '~/types/props/base-button/IBaseButtonProps';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	cy: '',
	outline: true,
	color: 'green',
	textId: 'confirm',
	icon: 'mdi-check',
});
</script>
