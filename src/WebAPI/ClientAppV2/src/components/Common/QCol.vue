<template>
	<div :class="classConfig">
		<slot />
	</div>
</template>

<script setup lang="ts">
interface QColProps {
	cols?: string | number;
	offset?: string | number;
	order?: string | number;
	align?: 'start' | 'center' | 'end' | 'stretch';
}

const props = withDefaults(defineProps<QColProps>(), {
	cols: 0,
	offset: '0',
	order: '0',
	align: 'start',
});

const classConfig = computed(() => {
	return {
		col: true,
		[`col-${props.cols}`]: props.cols > 0,
		[`offset-${props.offset}`]: true,
		[`order-${props.order}`]: true,
		[`align-${props.align}`]: true,
	};
});
</script>
