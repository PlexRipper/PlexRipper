<template>
	<v-tooltip :disabled="showTooltip" top>
		<template #activator="{ on, attrs }">
			<v-btn
				raised
				nuxt
				:class="getClass"
				:color="getColor"
				:outlined="getIsOutlined"
				:disabled="isDisabled"
				:loading="loading"
				:width="getWidth"
				:block="block"
				:depressed="depressed"
				:icon="iconMode"
				:href="href"
				:tile="tile"
				:to="to"
				:target="href ? '_blank' : '_self'"
				v-bind="attrs"
				:x-large="xLarge"
				:x-small="xSmall"
				v-on="on"
				@click="click($event)"
			>
				<v-icon v-if="getIcon" class="mx-2" :size="iconSize" :color="getColor">{{ getIcon }}</v-icon>
				<span v-if="getText !== ''">{{ $t(getText) }} </span>
				<slot></slot>
			</v-btn>
		</template>
		<span>{{ $t(getText) }}</span>
	</v-tooltip>
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
	readonly tooltipId!: string;

	@Prop({ required: false, type: String, default: '' })
	readonly icon!: string;

	@Prop({ required: false, type: String, default: '' })
	readonly color!: string;

	@Prop({ required: false, type: Boolean, default: false })
	readonly filled!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly tile!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly depressed!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly iconMode!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly disabled!: boolean;

	@Prop({ required: false, type: Boolean, default: false })
	readonly loading!: boolean;

	@Prop({ required: false, type: Number, default: 36 })
	readonly width!: number;

	@Prop({ required: false, type: Boolean, default: false })
	readonly block!: boolean;

	@Prop({ required: false, type: String })
	readonly href!: string;

	@Prop({ required: false, type: String })
	readonly to!: string;

	@Prop({ required: false, type: String })
	readonly iconSize!: string;

	@Prop({ required: false, type: Boolean })
	readonly xLarge!: boolean;

	@Prop({ required: false, type: Boolean })
	readonly xSmall!: boolean;

	get isDark(): boolean {
		return this.$vuetify.theme.dark;
	}

	get showTooltip(): boolean {
		return this.tooltipId === '';
	}

	get isDisabled(): boolean {
		return this.disabled;
	}

	get getWidth(): string | number {
		if (this.width !== 36) {
			return this.width;
		}
		return 'auto';
	}

	get getClass(): string[] {
		return [
			this.getIsFilled ? 'filled' : '',
			this.getIsOutlined ? 'outlined' : '',
			'p-btn',
			'mx-2',
			'i18n-formatting',
			this.buttonType === ButtonType.Alphabet ? 'navigation-btn' : '',
		];
	}

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
			case ButtonType.Download:
				return 'mdi-download';
			case ButtonType.Delete:
				return 'mdi-delete';
			case ButtonType.Error:
				return 'mdi-alert-circle';
			case ButtonType.ExternalLink:
				return 'mdi-open-in-new';
		}
		return '';
	}

	get getColor(): string {
		if (this.color) {
			return this.color;
		}
		switch (this.buttonType) {
			case ButtonType.None:
				return this.isDark ? 'white' : 'black';
			case ButtonType.Navigation:
				return this.isDark ? 'white' : 'black';
			case ButtonType.Cancel:
				return this.isDark ? 'white' : 'black';
			case ButtonType.ExternalLink:
				return this.isDark ? 'white' : 'black';
			case ButtonType.Confirm:
				return 'green';
			case ButtonType.Save:
				return 'green';
			case ButtonType.Delete:
				return 'red';
			case ButtonType.Error:
				return 'red';
		}
		return '';
	}

	get getIsOutlined(): boolean {
		switch (this.buttonType) {
			case ButtonType.ExternalLink:
			case ButtonType.Download:
			case ButtonType.Alphabet:
				return false;
			default:
				return true;
		}
	}

	get getIsFilled(): boolean {
		if (this.filled) {
			return this.filled;
		}
		switch (this.buttonType) {
			case ButtonType.Cancel:
			case ButtonType.Confirm:
			case ButtonType.Delete:
			case ButtonType.Alphabet:
				return false;
		}
		return false;
	}

	click(event: any): void {
		this.$emit('click', event);
	}
}
</script>
