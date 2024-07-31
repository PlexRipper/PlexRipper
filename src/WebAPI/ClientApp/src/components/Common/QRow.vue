<template>
	<div
		:class="classes"
		:data-cy="cy">
		<slot />
	</div>
</template>

<script setup lang="ts">
interface QRowProps {
	justify?: 'start' | 'center' | 'end' | 'around' | 'between';
	align?: 'start' | 'center' | 'end';
	gutter?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'none';
	cy?: string;
	noWrap?: boolean;
	wrap?: boolean;
	reverse?: boolean;
	column?: boolean;
	fullHeight?: boolean;
}

const props = withDefaults(defineProps<QRowProps>(), {
	justify: 'start',
	cy: '',
	align: 'center',
	gutter: 'none',
	noWrap: false,
	reverse: false,
	column: false,
});

const classes = computed(() => {
	const classList: string[] = ['row full-width'];

	if (props.column) {
		classList.push('column');
	}

	if (props.justify) {
		classList.push(`justify-${props.justify}`);
	}

	if (props.align) {
		classList.push(`items-${props.align}`);
	}

	if (props.gutter !== 'none') {
		classList.push(`q-gutter-${props.gutter}`);
	}

	if (props.wrap) {
		classList.push('wrap');
	} else if (props.noWrap) {
		classList.push('no-wrap');
	}

	if (props.reverse) {
		classList.push('reverse');
	}

	if (props.fullHeight) {
		classList.push('full-height');
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
