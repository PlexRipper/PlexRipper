<template>
	<div :class="classes" :style="styles">
		<slot />
	</div>
</template>

<script setup lang="ts">
import { computed, defineProps, withDefaults } from 'vue';

interface IBreakPoints {
	xs?: string | number | boolean;
	sm?: string | number | boolean;
	md?: string | number | boolean;
	lg?: string | number | boolean;
	xl?: string | number | boolean;
}

interface IOffset {
	offsetXs?: string | number | boolean;
	offsetSm?: string | number | boolean;
	offsetMd?: string | number | boolean;
	offsetLg?: string | number | boolean;
	offsetXl?: string | number | boolean;
}

interface QColProps extends IBreakPoints, IOffset {
	cols?: 'auto' | 'grow' | 'shrink' | string | number | boolean;
	offset?: string | number | boolean;
	width?: number;
	textAlign?: 'left' | 'center' | 'right' | 'justify';
	alignSelf?: 'auto' | 'start' | 'end' | 'center' | 'baseline' | 'stretch' | 'none';
}

const breakPoints: IBreakPoints = {
	xs: false,
	sm: false,
	md: false,
	lg: false,
	xl: false,
};

const props = withDefaults(defineProps<QColProps>(), {
	cols: 0,
	offset: false,
	alignSelf: 'none',
	width: 0,
	xs: false,
	sm: false,
	md: false,
	lg: false,
	xl: false,

	offsetXs: false,
	offsetSm: false,
	offsetMd: false,
	offsetLg: false,
	offsetXl: false,
});

const classes = computed(() => {
	const classList: string[] = [];

	if (props.cols) {
		classList.push(`col-${props.cols}`);
	} else {
		classList.push('col');
	}

	if (props.alignSelf && props.alignSelf !== 'none') {
		classList.push(`self-${props.alignSelf}`);
	}

	for (const key in breakPoints) {
		if (props[key]) {
			classList.push(`col-${key}-${props[key]}`);
		}
	}

	if (props.offset) {
		classList.push(`offset-${props.offset}`);
	}

	switch (props.textAlign) {
		case 'left':
			classList.push('text-left');
			break;
		case 'center':
			classList.push('text-center');
			break;
		case 'right':
			classList.push('text-right');
			break;
		case 'justify':
			classList.push('text-justify');
			break;
	}

	for (const key in breakPoints) {
		const offsetKey = `offset${key.charAt(0).toUpperCase()}${key.slice(1)}` as keyof IOffset;
		if (props[offsetKey]) {
			classList.push(`offset-${key}-${props[offsetKey]}`);
		}
	}

	return classList;
});

const styles = computed(() => {
	const styleList: Record<string, string> = {};

	if (props.width) {
		styleList.minWidth = `${props.width}px`;
		styleList.maxWidth = `${props.width}px`;
	}

	return styleList;
});
</script>
