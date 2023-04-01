<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import BaseButton from '@buttons/BaseButton.vue';
import { IBaseButtonProps } from '@props';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	width: 130,
	cy: '',
	outline: true,
	icon: 'mdi-bug-outline',
});
</script>
