import type { PropType } from 'vue';
import ButtonType from '@enums/buttonType';

export function baseBtnPropsDefault(): Record<string, any> {
	return {
		// region PlexRipper
		cy: {
			type: String,
			default: '',
		},
		type: {
			type: String as PropType<ButtonType>,
			default: ButtonType.None,
		},
		textId: {
			type: String,
			default: '',
		},
		tooltipId: {
			type: String,
			default: '',
		},
		width: {
			type: Number,
			default: 0,
		},
		height: {
			type: Number,
			default: 0,
		},
		iconAlign: {
			type: String as PropType<'left' | 'right'>,
			default: 'left',
		},
		vertical: {
			type: Boolean,
			default: false,
		},
		// endregion
		// region Quasar native

		label: {
			type: String,
			default: '',
		},
		icon: {
			type: String,
			default: undefined,
		},
		size: {
			type: String,
			default: 'normal',
		},
		flat: {
			type: Boolean,
			default: false,
		},
		glossy: {
			type: Boolean,
			default: false,
		},
		round: {
			type: Boolean,
			default: false,
		},
		rounded: {
			type: Boolean,
			default: false,
		},
		outline: {
			type: Boolean,
			default: true,
		},
		loading: {
			type: Boolean,
			default: false,
		},
		disabled: {
			type: Boolean,
			default: false,
		},
		push: {
			type: Boolean,
			default: false,
		},
		// endregion
		// region Vuetify native
		block: {
			type: Boolean,
			default: false,
		},
		// endregion
		iconSize: {
			type: Number,
			default: 16,
		},
		color: {
			type: String as PropType<'default' | 'positive' | 'warning' | 'negative'>,
			default: 'default',
		},
		href: {
			type: String,
			default: undefined,
		},
		to: {
			type: String,
			default: '',
			required: false,
		},
	};
}
