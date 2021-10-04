<template>
	<v-dialog :value="dialog" width="500" persistent @click:outside="cancel">
		<v-card>
			<v-card-title class="headline i18n-formatting">{{ $t(getText.title) }}</v-card-title>

			<v-card-text class="i18n-formatting">
				<p>{{ $t(getText.text) }}</p>
				<p v-if="getText.warning" class="text-center">
					<b>{{ $t(getText.warning) }}</b>
				</p>
			</v-card-text>

			<v-divider></v-divider>

			<v-card-actions>
				<p-btn :button-type="getCancelButtonType" @click="cancel" />
				<v-spacer></v-spacer>
				<p-btn :button-type="getConfirmationButtonType" :loading="loading" @click="confirm" />
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

	@Prop({ required: false, type: Boolean, default: false })
	readonly confirmLoading!: boolean;

	loading: boolean = false;

	get getText(): IText {
		if (this.textId === '') {
			return {
				id: 'null',
				title: 'Could not find the correct confirmation text..',
				text: 'Could not find the correct confirmation text..',
			};
		}
		const msg: any = this.$getMessage(`confirmation.${this.textId}`);
		return {
			id: this.textId,
			title: msg?.title ?? '',
			text: msg?.text ?? '',
			warning: msg?.warning ?? '',
		};
	}

	get getCancelButtonType(): ButtonType {
		return ButtonType.Cancel;
	}

	get getConfirmationButtonType(): ButtonType {
		return ButtonType.Confirm;
	}

	cancel(): void {
		this.$emit('cancel');
	}

	confirm(): void {
		this.$emit('confirm');
		if (this.confirmLoading) {
			this.loading = true;
		}
	}

	mounted() {
		this.$watchAsObservable('dialog').subscribe(() => {
			this.loading = false;
		});
	}
}
</script>
