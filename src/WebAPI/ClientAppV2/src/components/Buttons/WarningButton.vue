<template>
	<BaseButton v-bind="props"/>
</template>

<script setup lang="ts">
import {withDefaults, defineProps} from 'vue';
import BaseButton from '@buttons/BaseButton.vue';

interface WarningBtnProps {
	width?: number;
	icon?: string;
	cy?: string;
	label?: string;
}

const props = withDefaults(defineProps<WarningBtnProps>(), {
	width: 50,
	icon: 'mdi-alert',
	cy: '',
	label: 'Warning',
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
