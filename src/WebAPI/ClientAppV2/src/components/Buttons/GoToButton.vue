<template>
	<BaseButton v-bind="props" />
</template>

<script setup lang="ts">
import { withDefaults, defineProps } from 'vue';
import BaseButton from '@buttons/BaseButton.vue';
import { IBaseButtonProps } from '@props';

defineOptions({
	inheritAttrs: false,
});

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	width: 130,
});
</script>
