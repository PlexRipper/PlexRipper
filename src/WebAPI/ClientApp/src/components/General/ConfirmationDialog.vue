<template>
	<v-dialog v-model="dialog" width="500" persistent>
		<template #activator="{}">
			<p-btn
				:button-type="buttonType"
				:width="width"
				:block="block"
				:disabled="disabled"
				:text-id="buttonTextId"
				@click="openDialog"
			/>
		</template>

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
import PBtn from '@components/Extensions/PlexRipperButton.vue';
import IText from '@interfaces/IText';
import ButtonType from '@enums/buttonType';

@Component({
	components: {
		PBtn,
	},
})
export default class ConfirmationDialog extends Vue {
	@Prop({ required: true, type: String, default: '' })
	readonly textId!: string;

	@Prop({ required: true, type: String, default: '' })
	readonly buttonTextId!: string;

	@Prop({ required: false, type: Number, default: 150 })
	readonly width!: number;

	@Prop({ required: false, type: Boolean, default: false })
	readonly disabled!: boolean;

	@Prop({ required: false, type: String, default: ButtonType.None })
	readonly buttonType!: ButtonType;

	@Prop({ required: false, type: Boolean, default: false })
	readonly block!: boolean;

	dialog: boolean = false;

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
		this.dialog = false;
	}

	openDialog(): void {
		this.dialog = true;
	}

	cancel(): void {
		this.$emit('cancel');
	}

	confirm(): void {
		this.$emit('confirm');
	}
}
</script>
