<template>
	<v-row justify="center">
		<v-col cols="auto">
			<dark-mode-toggle />
		</v-col>
		<v-spacer />
		<v-col cols="auto">
			<v-btn color="primary" :width="150" @click="back"> Back </v-btn>
			<v-btn color="primary" :width="150" @click="next"> Continue </v-btn>
		</v-col>
		<v-spacer />
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import DarkModeToggle from '@components/General/DarkModeToggle.vue';

@Component<NavigationBar>({
	components: {
		DarkModeToggle,
	},
})
export default class NavigationBar extends Vue {
	back(): void {
		this.$emit('back');
	}

	next(): void {
		this.$emit('next');
	}
}
</script>
