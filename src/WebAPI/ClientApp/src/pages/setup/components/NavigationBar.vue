<template>
	<v-row justify="center">
		<v-col cols="auto" style="position: absolute; left: 0">
			<dark-mode-toggle />
		</v-col>
		<v-spacer />
		<v-col v-if="!isLast" cols="2">
			<p-btn class="mx-2" text-id="back" :block="true" :disabled="disableBack" :button-type="backButtonType" @click="back" />
		</v-col>
		<v-col v-if="!isLast" cols="2">
			<p-btn class="mx-2" text-id="next" :block="true" :disabled="disableNext" :button-type="forwardButtonType" @click="next" />
		</v-col>
		<v-col v-else cols="3">
			<p-btn class="mx-2" text-id="finish-setup" :block="true" :button-type="skipButtonType" @click="finishSetup" />
		</v-col>
		<v-spacer />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import DarkModeToggle from '@components/General/DarkModeToggle.vue';
import PBtn from '@components/Extensions/PlexRipperButton.vue';
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

	forwardButtonType: ButtonType = ButtonType.Forward;
	backButtonType: ButtonType = ButtonType.Back;
	skipButtonType: ButtonType = ButtonType.Skip;

	back(): void {
		this.$emit('back');
	}

	next(): void {
		this.$emit('next');
	}

	finishSetup(): void {
		this.$emit('finish');
	}
}
</script>
