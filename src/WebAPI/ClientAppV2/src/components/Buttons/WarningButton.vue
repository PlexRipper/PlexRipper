<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import BaseButton from '@buttons/BaseButton.vue';
import { IBaseButtonProps } from '~/types/props/base-button/IBaseButtonProps';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	icon: 'mdi-alert',
	textId: 'warning',
	color: 'amber-10',
});
</script>

<style lang="scss">
.warning-btn {
	color: #ff9800;
	border-color: #ff9800;
	background-color: transparent;
}
</style>
