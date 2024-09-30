<template>
	<div :class="divClasses">
		<slot name="prepend" />
		<span
			:class="spanClasses"
			:data-cy="cy">
			<slot name="default">
				{{ value }}
			</slot>
		</span>
		<slot name="append" />
	</div>
</template>

<script lang="ts" setup>
import type { IQTextProps } from '@interfaces';

const props = withDefaults(defineProps<IQTextProps>(), {
	value: '',
	size: 'body1',
	type: 'primary',
	align: 'left',
	bold: 'regular',
	cy: '',
	fullWidth: false,
});

const divClasses = computed(() => ({
	'q-text-container': true,
	[`text-${props.align}`]: true,
	[`full-width`]: props.fullWidth,
}));

const spanClasses = computed(() => ({
	'q-text': true,
	[`text-${props.size}`]: true,
	[`text-weight-${props.bold}`]: true,
	[`full-width`]: props.fullWidth,
}));
</script>

<style lang="scss">
.q-text-container {}
</style>
