<template>
	<BaseButton v-bind="props" class="warning-btn" label="Standard" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import { IBaseButtonProps } from '~/types/props/base-button/IBaseButtonProps';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	icon: 'mdi-plus-box-outline',
	size: 'xl',
});
</script>
