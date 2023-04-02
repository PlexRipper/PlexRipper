<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import BaseButton from '@buttons/BaseButton.vue';
import { IBaseButtonProps } from '@props';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	textId: 'skip-setup',
	icon: 'mdi-debug-step-over',
	iconAlign: 'right',
	outline: true,
});
</script>
