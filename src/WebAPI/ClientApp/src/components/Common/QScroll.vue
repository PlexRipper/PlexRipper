<template>
	<q-scroll-area
		:style="{ 'height': props.fit ? 'none' : props.height, 'max-width': props.fit ? 'none' : props.width }"
		:class="[props.fit ? 'fit' : '']"
		:thumb-style="thumbStyle"
		:bar-style="barStyle">
		<slot />
	</q-scroll-area>
</template>

<script setup lang="ts">
import type { VueStyleObjectProp } from 'quasar';

const props = withDefaults(defineProps<{
	fit?: boolean;
	height?: string | number;
	width?: string | number;
}>(), {
	fit: true,
	height: '400px',
	width: '300px',
});

const thumbStyle: VueStyleObjectProp = {
	right: '4px',
	borderRadius: '5px',
	backgroundColor: '#d20000',
	width: '5px',
	opacity: '0.75',
};

const barStyle: VueStyleObjectProp = {
	right: '2px',
	borderRadius: '9px',
	backgroundColor: 'rgba(0,0,0,0)',
	width: '9px',
	opacity: '0.2',
};
</script>
