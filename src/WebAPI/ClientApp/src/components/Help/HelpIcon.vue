<template>
	<v-row justify="space-between" class="flex-nowrap" no-gutters>
		<v-col>
			<v-subheader class="form-label text-no-wrap">{{ getLabel }}</v-subheader>
		</v-col>
		<v-col v-if="hasHelpPage" cols="auto">
			<v-btn style="margin: 6px" icon @click="openDialog">
				<v-icon :size="24"> mdi-help-circle-outline </v-icon>
			</v-btn>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { HelpService } from '@service';

interface IHelp {
	label: string;
	title: string;
	text: string;
}

@Component<HelpIcon>({})
export default class HelpIcon extends Vue {
	@Prop({ required: false, type: String, default: '' })
	readonly labelId!: string;

	@Prop({ required: true, type: String, default: '' })
	readonly helpId!: string;

	get getLabel(): string {
		return this.$ts(`${this.helpId}.label`);
	}

	get hasHelpPage(): boolean {
		if (this.helpId) {
			const msgObject = this.$getMessage(this.helpId) as IHelp;
			// Complains about returning string if I return directly, instead of an if statement returning true
			if (msgObject && msgObject.title && msgObject.text) {
				return true;
			}
		}
		return false;
	}

	openDialog(): void {
		HelpService.openHelpDialog(this.helpId);
	}
}
</script>
