<template>
	<v-row justify="space-between" class="flex-nowrap" no-gutters>
		<v-col>
			<v-subheader class="form-label text-no-wrap">{{ getLabel }}:</v-subheader>
		</v-col>
		<v-col v-if="hasHelpPage" cols="auto">
			<v-btn style="margin: 8px" icon @click="openDialog">
				<v-icon :size="24"> mdi-help-circle-outline </v-icon>
			</v-btn>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { HelpService } from '@state';

@Component
export default class HelpIcon extends Vue {
	@Prop({ required: false, type: String, default: '' })
	readonly labelId!: string;

	@Prop({ required: false, type: String, default: '' })
	readonly helpId!: string;

	get getLabel(): string {
		if (this.hasHelpPage) {
			return this.$ts(`${this.helpId}.label`);
		} else {
			return this.$ts(this.helpId);
		}
	}

	get hasHelpPage(): boolean {
		if (this.helpId) {
			// Is true if it has an object with label, title and text
			return typeof this.$getMessage(this.helpId) === 'object';
		}
		return false;
	}

	openDialog(): void {
		HelpService.openHelpDialog(this.helpId);
	}
}
</script>
