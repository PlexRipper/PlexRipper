<template>
	<QRow
		:class="divClasses"
		align="center">
		<QCol
			v-if="$slots['prepend']"
			cols="auto">
			<slot name="prepend" />
		</QCol>
		<QCol :text-align="align">
			<span
				:class="spanClasses"
				:data-cy="cy">
				<slot name="default">
					{{ value }}
				</slot>
			</span>
		</QCol>
		<QCol
			v-if="$slots['append']"
			cols="auto">
			<slot name="append" />
		</QCol>
	</QRow>
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

const spanClasses = computed(() => {
	let bold = props.bold;
	if (props.bold === '') {
		bold = 'bold';
	}
	return ({
		'q-text': true,
		[`text-${props.size}`]: true,
		[`text-weight-${bold}`]: true,
		[`full-width`]: props.fullWidth,
	});
});
</script>
