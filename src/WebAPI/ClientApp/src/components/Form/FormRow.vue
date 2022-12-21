<template>
	<v-row class="flex-nowrap" no-gutters>
		<v-col cols="3">
			<v-subheader class="form-label text-no-wrap">{{ getLabel }}</v-subheader>
		</v-col>
		<v-col cols="1">
			<HelpButton v-if="hasHelpPage" @click="openDialog" />
		</v-col>
		<v-col cols="8" align-self="end">
			<slot />
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { HelpService } from '@service';

interface IHelp {
	label: string;
	title: string;
	text: string;
}

@Component
export default class FormRow extends Vue {
	@Prop({ required: true, type: String, default: '' })
	readonly formId!: string;

	get getLabel(): string {
		return this.$ts(`${this.formId}.label`);
	}

	get hasHelpPage(): boolean {
		if (this.formId) {
			const msgObject = this.$getMessage(this.formId) as IHelp;
			// Complains about returning string if I return directly, instead of an if statement returning true
			if (msgObject && msgObject.title && msgObject.text) {
				return true;
			}
		}
		return false;
	}

	openDialog(): void {
		HelpService.openHelpDialog(this.formId);
	}
}
</script>
