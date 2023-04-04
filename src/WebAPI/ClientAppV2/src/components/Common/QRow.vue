<template>
	<div :class="classes">
		<slot />
	</div>
</template>

<script setup lang="ts">
import { computed, defineProps, withDefaults } from 'vue';

interface QRowProps {
	justify?: 'start' | 'center' | 'end' | 'around' | 'between' | boolean;
	align?: 'start' | 'center' | 'end' | boolean;
	gutter?: string | boolean;
	noWrap?: boolean;
	wrap?: boolean;
	reverse?: boolean;
	column?: boolean;
}

const props = withDefaults(defineProps<QRowProps>(), {
	justify: false,
	align: false,
	gutter: false,
	noWrap: false,
	reverse: false,
	column: false,
});

const classes = computed(() => {
	const classList: string[] = ['row'];

	if (props.column) {
		classList.push('column');
	}

	if (props.justify) {
		classList.push(`justify-${props.justify}`);
	}

	if (props.align) {
		classList.push(`items-${props.align}`);
	}

	if (props.gutter) {
		classList.push(`gutter-${props.gutter}`);
	}

	if (props.wrap) {
		classList.push('wrap');
	} else if (props.noWrap) {
		classList.push('no-wrap');
	}

	if (props.reverse) {
		classList.push('reverse');
	}

	return classList;
});
</script>

<style lang="scss">
.row {
	// Also used internally in Quasar components such as QInput and messes up the layout
	// width: 100%;
}
</style>
