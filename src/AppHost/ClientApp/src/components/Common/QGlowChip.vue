<template>
	<q-chip
		:clickable="clickable"
		:color="color"
		:size="size"
		:style="styles"
		:class="{ 'q-chip-background': hasBackground }"
		outline>
		<QText
			:text-color="textColor"
			:value="value" />
	</q-chip>
</template>

<script lang="ts" setup>
import type { NamedColor } from 'quasar';
import { getCssVar } from 'quasar';

const props = withDefaults(defineProps<{
	value: string | number;
	color?: NamedColor;
	textColor?: NamedColor;
	size?: string;
	clickable?: boolean;
	hasBackground?: boolean;
}>(), {
	color: 'primary',
	size: 'md',
	clickable: false,
	hasBackground: false,
});

const styles = computed(() => {
	return {
		'box-shadow': getCssVar(props.color),
	};
});
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;

.q-chip {
  border-width: 2px;
  box-shadow: 0 0 10px 2px;
  transition: all 0.25s ease;
}

.q-chip-background {
  background-color: $dark-xl-background-color !important;
}
</style>
