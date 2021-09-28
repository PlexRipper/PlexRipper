<template>
	<v-dialog v-model="dialog" width="500" persistent>
		<v-card>
			<v-card-title class="headline i18n-formatting">{{ $t(getText.title) }}</v-card-title>

			<v-card-text class="i18n-formatting">{{ $t(getText.text) }} </v-card-text>

			<v-divider></v-divider>

			<v-card-actions>
				<p-btn :button-type="getCancelButtonType" @click="closeDialog" />
				<v-spacer></v-spacer>
				<p-btn :button-type="getConfirmationButtonType" @click="confirm" />
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import IText from '@interfaces/IText';
import ButtonType from '@enums/buttonType';

@Component<ConfirmationDialog>({})
export default class ConfirmationDialog extends Vue {
	/**
	 * The Vue-i18n text id used for the confirmation window that pops-up.
	 * @type {string}
	 */
	@Prop({ required: true, type: String, default: '' })
	readonly textId!: string;

	@Prop({ required: true, type: Boolean })
	readonly dialog!: boolean;

	get getText(): IText {
		if (this.textId === '') {
			return {
				id: 'null',
				title: 'Could not find the correct confirmation text..',
				text: 'Could not find the correct confirmation text..',
			};
		}
		return {
			id: this.textId,
			title: `confirmation.${this.textId}.title`,
			text: `confirmation.${this.textId}.text`,
		};
	}

	get getCancelButtonType(): ButtonType {
		return ButtonType.Cancel;
	}

	get getConfirmationButtonType(): ButtonType {
		return ButtonType.Confirm;
	}

	closeDialog(): void {
		this.$emit('close');
	}

	cancel(): void {
		this.$emit('cancel');
	}

	confirm(): void {
		this.$emit('confirm');
	}
}
</script>
