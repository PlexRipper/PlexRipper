<template>
	<v-row justify="center">
		<v-col cols="auto" style="position: absolute; left: 0">
			<dark-mode-toggle />
		</v-col>
		<v-spacer />
		<v-col v-if="!isLast" cols="2">
			<p-btn
				class="mx-2"
				text-id="back"
				:block="true"
				:disabled="disableBack"
				:button-type="getNavigationButtonType"
				@click="back"
			/>
		</v-col>
		<v-col v-if="!isLast" cols="2">
			<p-btn
				class="mx-2"
				text-id="next"
				:block="true"
				:disabled="disableNext"
				:button-type="getNavigationButtonType"
				@click="next"
			/>
		</v-col>
		<v-col v-else cols="3">
			<p-btn class="mx-2" text-id="finish-setup" :block="true" to="/" :button-type="getNavigationButtonType" />
		</v-col>
		<v-spacer />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import DarkModeToggle from '@components/General/DarkModeToggle.vue';
import PBtn from '@components/General/PlexRipperButton.vue';
import ButtonType from '@enums/buttonType';

@Component({
	components: {
		DarkModeToggle,
		PBtn,
	},
})
export default class NavigationBar extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly disableBack!: boolean;

	@Prop({ required: true, type: Boolean })
	readonly disableNext!: boolean;

	@Prop({ required: true, type: Boolean })
	readonly isLast!: boolean;

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
