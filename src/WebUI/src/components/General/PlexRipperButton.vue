<template>
	<v-btn
		:class="[getIsFilled ? 'filled' : '', 'p-btn', 'mx-2', 'i18n-formatting']"
		:color="getColor"
		outlined
		raised
		:disabled="disabled"
		:loading="loading"
		:width="width"
		@click="click"
	>
		<v-icon v-if="getIcon" class="mx-2" :color="getColor">{{ getIcon }}</v-icon>
		<span v-if="getText !== ''">{{ $t(getText) }}</span>
	</v-btn>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import ButtonType from '@enums/buttonType';

@Component
export default class PBtn extends Vue {
	@Prop({ required: false, type: String, default: ButtonType.None })
	readonly buttonType!: ButtonType;

	@Prop({ required: false, type: String, default: '' })
	readonly textId!: string;

	@Prop({ required: false, type: String, default: '' })
	readonly icon!: string;

	@Prop({ required: false, type: String, default: '' })
	readonly color!: string;

	@Prop({ required: false, type: Boolean, default: false })
	readonly filled!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly disabled!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly loading!: boolean;

	@Prop({ required: false, type: Number, default: 150 })
	readonly width!: number;

	get getText(): string {
		const prefix = 'general.commands.';
		if (this.textId !== '') {
			return prefix + this.textId;
		}
		switch (this.buttonType) {
			case ButtonType.Cancel:
				return prefix + 'cancel';
			case ButtonType.Confirm:
				return prefix + 'confirm';
			case ButtonType.Delete:
				return prefix + 'delete';
			case ButtonType.Error:
				return prefix + 'error';
		}
		return '';
	}

	get getIcon(): string {
		if (this.icon) {
			return this.icon;
		}
		switch (this.buttonType) {
			case ButtonType.Cancel:
				return 'mdi-cancel';
			case ButtonType.Confirm:
				return 'mdi-check';
			case ButtonType.Save:
				return 'mdi-content-save';
			case ButtonType.Delete:
				return 'mdi-delete';
			case ButtonType.Error:
				return 'mdi-alert-circle';
		}
		return '';
	}

	get getColor(): string {
		if (this.color) {
			return this.color;
		}
		switch (this.buttonType) {
			case ButtonType.None:
				return 'white';
			case ButtonType.Navigation:
				return 'white';
			case ButtonType.Cancel:
				return 'white';
			case ButtonType.Confirm:
				return 'green';
			case ButtonType.Save:
				return 'green';
			case ButtonType.Delete:
				return 'red';
		}
		return 'white';
	}

	get getIsFilled(): boolean {
		if (this.filled) {
			return this.filled;
		}
		switch (this.buttonType) {
			case ButtonType.Cancel:
			case ButtonType.Confirm:
			case ButtonType.Delete:
				return false;
		}
		return false;
	}

	click(): void {
		this.$emit('click');
	}
}
</script>
