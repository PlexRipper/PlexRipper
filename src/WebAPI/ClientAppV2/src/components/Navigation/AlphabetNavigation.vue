<template>
	<v-col cols="auto" class="d-flex flex-column pt-2">
		<v-btn
			v-for="letter in alphabet"
			:key="letter"
			:width="20"
			class="navigation-btn filled"
			tile
			depressed
			@click="scrollTo(letter)"
		>
			<span>{{ letter }}</span>
		</v-btn>
	</v-col>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';

@Component
export default class AlphabetNavigation extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	get alphabet(): string[] {
		const numeric: string = '!@0123456789';
		const alphabet: string = '#ABCDEFGHIJKLMNOPQRSTUVWXYZ';
		const availableNavigation: string[] = [];

		// Check for occurrence of title with numeric/special character
		for (let i = 1; i < numeric.length; i++) {
			if (this.items.some((x) => x.title.startsWith(numeric[i]))) {
				availableNavigation.push('#');
				break;
			}
		}

		// Check for occurrence of title with alphabetic character
		for (let i = 1; i < alphabet.length; i++) {
			if (this.items.some((x) => x.title.startsWith(alphabet[i]))) {
				availableNavigation.push(alphabet[i]);
			}
		}
		return availableNavigation;
	}

	scrollTo(letter: string): void {
		this.$emit('scroll-to', letter);
	}
}
</script>
