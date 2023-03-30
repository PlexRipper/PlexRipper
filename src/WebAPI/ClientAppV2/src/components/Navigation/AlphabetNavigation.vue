<template>
	<q-col cols="auto" class="alphabet-navigation column wrap">
		<q-btn v-for="letter in alphabet" :key="letter" class="navigation-btn" :label="letter" flat @click="scrollTo(letter)" />
	</q-col>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, withDefaults, computed } from 'vue';

const props = withDefaults(
	defineProps<{
		scrollDict: Record<string, number>;
	}>(),
	{},
);

const emit = defineEmits<{ (e: 'scroll-to', letter: string): void }>();

const alphabet = computed((): string[] => {
	return Object.keys(props.scrollDict);
});

function scrollTo(letter: string): void {
	emit('scroll-to', letter);
}
</script>
<style lang="scss">
.navigation-btn {
	height: 32px;
	width: 24px;
	background: transparent !important;
	flex: 1 1 24px !important;
	text-align: center;
	font-weight: bold;

	&.filled {
		background-color: currentColor;
	}

	&.outlined {
		border: 2px solid currentColor;
	}

	&.theme--dark {
		color: red;
	}

	&.theme--light {
		color: darkred;
	}

	&:hover {
		&::before {
			opacity: 0.2 !important;
		}
	}
}
</style>
