<template>
	<v-row justify="center">
		<v-col cols="auto" style="position: absolute; left: 0">
			<dark-mode-toggle />
		</v-col>
		<v-spacer />
		<v-col cols="auto">
			<p-btn class="mx-2" text-id="back" :button-type="getNavigationButtonType" @click="back" />
			<p-btn class="mx-2" text-id="next" :button-type="getNavigationButtonType" @click="next" />
		</v-col>
		<v-spacer />
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import DarkModeToggle from '@components/General/DarkModeToggle.vue';
import PBtn from '@components/General/PlexRipperButton.vue';
import ButtonType from '@enums/buttonType';

@Component<NavigationBar>({
	components: {
		DarkModeToggle,
		PBtn,
	},
})
export default class NavigationBar extends Vue {
	get getNavigationButtonType(): ButtonType {
		return ButtonType.Navigation;
	}

	back(): void {
		this.$emit('back');
	}

	next(): void {
		this.$emit('next');
	}
}
</script>
