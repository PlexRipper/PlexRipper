import { PropType, defineProps } from 'vue';
import ButtonType from '@enums/buttonType';

export function baseBtnProps() {
	return defineProps(baseBtnPropsDefault());
}

export function baseBtnPropsDefault() {
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
		// endregion
		// region Quasar native

		label: {
			type: String,
			default: '',
		},
		icon: {
			type: String,
			default: '',
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

		// endregion
		// region Vuetify native
		block: {
			type: Boolean,
			default: false,
		},
		iconOnly: {
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
			default: '',
		},
		to: {
			type: String,
			default: '',
			required: false,
		},
	};
}
