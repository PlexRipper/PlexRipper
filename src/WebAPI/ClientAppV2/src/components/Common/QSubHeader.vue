<template>
	<div :class="classes">
		<slot />
	</div>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';

const props = defineProps<{
	bold?: boolean;
}>();

const classes = computed(() => {
	return {
		'q-sub-header': true,
		'text-weight-bold': props.bold,
	};
});
</script>
<style lang="scss">
.q-sub-header {
	align-items: center;
	display: flex;
	height: 48px;
	font-size: 0.875rem;
	padding: 0 16px 0 16px;
}
</style>
