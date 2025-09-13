<template>
	<q-icon
		:class="[`${kebabCase(valid)}-icon`, 'q-ma-sm']"
		size="30px"
		:color="getData.color"
		:name="getData.icon"
		:data-cy="cy">
		<q-tooltip
			v-if="getData.text"
			anchor="top middle"
			self="bottom middle"
			:offset="[10, 10]"
			:disabled="valid">
			<span>{{ getData.text }}</span>
		</q-tooltip>
	</q-icon>
</template>

<script setup lang="ts">
import { ValidationLevel } from '@enums';
import { kebabCase } from 'lodash-es';

const props = withDefaults(
	defineProps<{
		valid: ValidationLevel;
		text?: string;
		validText?: string;
		warningText?: string;
		invalidText?: string;
		cy?: string;
	}>(),
	{
		text: '',
		validText: '',
		warningText: '',
		invalidText: '',
		cy: 'valid-icon',
	},
);

const getData = computed((): {
	text: string;
	color: string;
	icon: string;
} => {
	switch (props.valid) {
		case ValidationLevel.Valid:
			return {
				color: 'green', icon: 'mdi-check-circle-outline', text: props.text !== '' ? props.text : props.validText,
			};
		case ValidationLevel.Warning:
			return {
				color: 'yellow', icon: 'mdi-alert-outline', text: props.text !== '' ? props.text : props.warningText,
			};
		case ValidationLevel.Invalid:
			return {
				color: 'red', icon: 'mdi-alert-circle', text: props.text !== '' ? props.text : props.invalidText,
			};
		default:
			return {
				color: '', icon: 'mdi-unknown', text: props.text !== '' ? props.text : props.validText,
			};
	}
});
</script>
