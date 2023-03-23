<template>
	<div :class="classes">
		<slot />
	</div>
</template>

<script setup lang="ts">
interface QRowProps {
	justify?: 'start' | 'center' | 'end' | 'around' | 'between' | boolean;
	align?: 'start' | 'center' | 'end' | boolean;
	gutter?: string;
	noWrap?: boolean;
	reverse?: boolean;
}

const props = withDefaults(defineProps<QRowProps>(), {
	justify: false,
	align: 'start',
	gutter: '0',
	noWrap: false,
	reverse: false,
});

const classes = computed(() => {
	const classList: string[] = ['row'];

	if (props.justify) {
		classList.push(`justify-${props.justify}`);
	}

	if (props.align) {
		classList.push(`items-${props.align}`);
	}

	if (props.gutter) {
		classList.push(`gutter-${props.gutter}`);
	}

	if (props.noWrap) {
		classList.push('no-wrap');
	}

	if (props.reverse) {
		classList.push('reverse');
	}

	return classList;
});
</script>
