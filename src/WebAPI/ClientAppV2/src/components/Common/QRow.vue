<template>
	<div :class="classConfig">
		<slot />
	</div>
</template>

<script setup lang="ts">
interface QRowProps {
	justify?: 'start' | 'center' | 'end' | 'around' | 'between';
	align?: 'start' | 'center' | 'end' | 'stretch';
	gutter?: string;
	noWrap?: boolean;
	reverse?: boolean;
}

const props = withDefaults(defineProps<QRowProps>(), {
	justify: 'start',
	align: 'start',
	gutter: '0',
	noWrap: false,
	reverse: false,
});

const classConfig = computed(() => {
	return {
		row: true,
		[`justify-${props.justify}`]: true,
		[`align-${props.align}`]: true,
		[`gutter-${props.gutter}`]: true,
		'no-wrap': props.noWrap,
		reverse: props.reverse,
	};
});
</script>
