<template>
	<q-badge
		outline
		:color="color"
		:style="styles"
		:size="size"
		floating
		transparent
		rounded>
		<QText
			:value="value"
			:text-color="textColor" />
	</q-badge>
</template>

<script setup lang="ts">
import type { NamedColor } from 'quasar';
import { getCssVar } from 'quasar';

const props = withDefaults(defineProps<{
	value: string | number;
	color?: NamedColor;
	textColor?: NamedColor;
	size?: string;
}>(), {
	color: 'primary',
	size: 'md',
});

const styles = computed(() => {
	return {
		'box-shadow': getCssVar(props.color),
	};
});
</script>

<style lang="scss">
.q-chip--outline {
  border-width: 2px;
  box-shadow: 0 0 10px 2px;
}
</style>
