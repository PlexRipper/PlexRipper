<template>
	<q-col cols="auto" class="d-flex flex-column pt-2">
		<q-btn
			v-for="letter in alphabet"
			:key="letter"
			:width="20"
			class="navigation-btn filled"
			flat
			@click="scrollTo(letter)"
		>
			<span>{{ letter }}</span>
		</q-btn>
	</q-col>
</template>

<script setup lang="ts">

import ITreeViewItem from "@class/ITreeViewItem";

const props = withDefaults(defineProps<{ items: ITreeViewItem[] }>(), {
	items: () => [],
});

const emit = defineEmits<{ (e: 'scroll-to', letter: string): void }>();

const alphabet = computed((): string[] => {
	const numeric: string = '!@0123456789';
	const alphabet: string = '#ABCDEFGHIJKLMNOPQRSTUVWXYZ';
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
