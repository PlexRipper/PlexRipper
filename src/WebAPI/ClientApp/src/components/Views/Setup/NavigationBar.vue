<template>
	<v-row justify="center">
		<v-col cols="auto" style="position: absolute; left: 0">
			<dark-mode-toggle />
		</v-col>
		<v-spacer />
		<v-col v-if="!isLast" cols="2">
			<NavigationPreviousButton class="mx-2" :disabled="disableBack" cy="setup-page-previous-button" @click="back" />
		</v-col>
		<v-col v-if="!isLast" cols="2">
			<NavigationNextButton class="mx-2" :disabled="disableNext" cy="setup-page-next-button" @click="next" />
		</v-col>
		<v-col v-else cols="3">
			<NavigationFinishSetupButton class="mx-2" cy="setup-page-skip-setup-button" @click="finishSetup" />
		</v-col>
		<v-spacer />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class NavigationBar extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly disableBack!: boolean;

	@Prop({ required: true, type: Boolean })
	readonly disableNext!: boolean;

	@Prop({ required: true, type: Boolean })
	readonly isLast!: boolean;

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
