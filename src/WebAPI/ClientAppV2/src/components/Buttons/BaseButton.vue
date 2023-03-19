<template>
	<q-btn
		:class="classConfig"
		:style="styleConfig"
		:flat="true"
		:rounded="false"
		:glossy="true"
		:label="getButtonText()"
		:outline="props.outline"
		:color="getColor()"
		:icon="props.icon"
		:disable="props.disabled"
		:loading="props.loading"
		:data-cy="props.cy">
		<q-tooltip v-if="getTooltip()" anchor="top middle" self="bottom middle" :offset="[10, 10]">
			<span>{{ getTooltip() }}</span>
		</q-tooltip>
	</q-btn>
</template>

<script setup lang="ts">
import { defineProps, withDefaults, reactive, toRef } from 'vue';
import { useI18n } from '#imports';

import ButtonType from '@enums/buttonType';
import type { IBaseButtonProps } from '~/types/props/base-button/IBaseButtonProps';

const props = withDefaults(defineProps<IBaseButtonProps>(), {
	// Base
	cy: '',
	lightColor: 'black',
	darkColor: 'white',

	// Style
	width: 0,
	height: 0,
	// Quasar
	icon: '',
	label: '',
	disabled: false,
	loading: false,
	outline: true,
	// Vuetify
	block: false,
	textId: 'missing-text',
	tooltipId: '',
	iconSize: undefined,
	iconOnly: false,
	href: '',
	to: '',
	type: ButtonType.None,
	size: 'normal',
});

const classConfig = {
	'p-btn': false,
	'i18n-formatting': true,
};

const styleConfig = {
	height: props.height > 0 ? props.height + 'px' : 'auto',
	width: props.width > 0 ? props.width + 'px' : 'auto',
	color: getColor(),
	border: props.outline ? '2px solid ' + getColor() : 'none',
};

function getButtonText(): string | number {
	return props.textId ? useI18n().t(`general.commands.${props.textId}`) : props.label;
}

function getColor(): string {
	if (props.color) {
		return props.color;
	}

	return (isDark() ? props.darkColor : props.lightColor) ?? '';
}

function getTooltip() {
	if (!props.tooltipId) {
		return null;
	}
	return useI18n().t(props.tooltipId);
}
</script>
