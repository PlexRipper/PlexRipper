<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import BaseButton from '@buttons/BaseButton.vue';
import { IBaseButtonProps } from '@props';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	textId: 'finish-setup',
	icon: 'mdi-check-circle-outline',
	iconAlign: 'right',
	color: 'positive',
	outline: true,
});
</script>
