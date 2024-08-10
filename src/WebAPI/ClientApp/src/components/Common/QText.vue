<template>
	<div :class="divClasses">
		<span :class="spanClasses">
			{{ value }}
			<slot />
		</span>
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
});

const divClasses = computed(() => ({
	'q-text-container': true,
	[`text-${props.align}`]: true,
}));

const spanClasses = computed(() => ({
	'q-text': true,
	[`text-${props.size}`]: true,
	[`text-weight-${props.bold}`]: true,
}));
</script>

<style lang="scss">
.q-text-container {
	width: 100%;
	padding: 0.5rem 0;
}
</style>
