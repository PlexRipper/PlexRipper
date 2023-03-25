<template>
	<q-col cols="auto" class="alphabet-navigation column wrap">
		<q-btn v-for="letter in alphabet" :key="letter" class="navigation-btn" :label="letter" flat @click="scrollTo(letter)" />
	</q-col>
</template>

<script setup lang="ts">
import ITreeViewItem from '@class/ITreeViewItem';

const props = withDefaults(defineProps<{ items: ITreeViewItem[] }>(), {
	items: () => [],
});

const emit = defineEmits<{ (e: 'scroll-to', letter: string): void }>();

const alphabet = computed((): string[] => {
	const numeric = '!@0123456789';
	const alphabet = '#ABCDEFGHIJKLMNOPQRSTUVWXYZ';
	const availableNavigation: string[] = [];

	// Check for occurrence of title with numeric/special character
	for (let i = 1; i < numeric.length; i++) {
		if (props.items.some((x) => x.title.startsWith(numeric[i]))) {
			availableNavigation.push('#');
			break;
		}
	}
	// Check for occurrence of title with alphabetic character
	for (let i = 1; i < alphabet.length; i++) {
		if (props.items.some((x) => x.title.startsWith(alphabet[i]))) {
			availableNavigation.push(alphabet[i]);
		}
	}
	return availableNavigation;
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
