<script lang="ts">
import Vue, { VNode, CreateElement, PropType, VNodeChildren } from 'vue';
import { RenderContext } from 'vue/types/options';
import VBtn from 'vuetify/lib/components/VBtn';
import VTooltip from 'vuetify/lib/components/VTooltip';
import VIcon from 'vuetify/lib/components/VIcon';
import Log from 'consola';
import ButtonType from '@enums/buttonType';
import Convert from '@mediaOverview/MediaTable/types/Convert';

export interface IBaseButtonProps {
	block: boolean;
	disabled: boolean;
	outlined: boolean;
	filled: boolean;
	loading: boolean;
	textId: string;
	type: ButtonType | String;
	tooltipId: string;
	icon: string;
	iconSize: number;
	iconAlign: 'Left' | 'Right';
	iconOnly: boolean;
	color: string;
	lightColor: string;
	darkColor: string;
	href: string;
	to: string;
	width: number;
	height: number;
	size: 'x-small' | 'small' | 'normal' | 'large' | 'x-large';
}

export default Vue.extend<IBaseButtonProps>({
	name: 'BaseButton',
	functional: true,
	props: {
		block: {
			type: Boolean,
			default: false,
		},
		disabled: {
			type: Boolean,
		},
		outlined: {
			type: Boolean,
			default: true,
		},
		filled: {
			type: Boolean,
		},
		loading: {
			type: Boolean,
		},
		textId: {
			type: String,
			default: 'DEFAULT TEXT',
		},
		/**
		 * The Vue-i18n text id used for the text inside the tooltip.
		 * @type {string}
		 */
		tooltipId: {
			type: String,
			default: '',
		},
		icon: {
			type: String,
			default: '',
		},
		iconSize: {
			type: Number,
			default: undefined,
		},
		iconAlign: {
			type: String as PropType<'Left' | 'Right'>,
			default: 'Left',
		},
		iconOnly: {
			type: Boolean,
		},
		href: {
			type: String,
			default: '',
		},
		to: {
			type: String,
			default: '',
		},
		type: {
			type: String as PropType<ButtonType>,
			default: '',
		},
		color: {
			type: String,
			default: '',
		},
		lightColor: {
			type: String,
			default: 'black',
		},
		darkColor: {
			type: String,
			default: 'white',
		},
		width: {
			type: Number,
			default: 36,
		},
		height: {
			type: Number,
			default: 36,
		},
		size: {
			type: String as PropType<'x-small' | 'small' | 'normal' | 'large' | 'x-large'>,
			default: 'normal',
		},
	},
	render(h: CreateElement, context: RenderContext<IBaseButtonProps>): VNode {
		const isDark = context.parent.$vuetify.theme.dark;
		const props = context.props;
		const size = getSize(props);
		Log.info('PROPSSIZE', props.size);
		Log.info('SIZE', size);
		return h(
			VTooltip,
			{
				props: { disabled: context.props.tooltipId === '', top: true },
				scopedSlots: {
					activator: ({ on, attrs }) => {
						return h(
							VBtn,
							{
								...context.data,
								class: {
									...(context.data.staticClass && {
										[context.data.staticClass]: true,
									}),
									'p-btn': true,
									'mx-2': true,
									'i18n-formatting': true,
									filled: props.filled,
									outlined: props.outlined,
								},
								on: {
									// Ensure we pass in the toolTip events
									...on,
									// Ensure we pass in the parent custom button events
									...context.data.on,
								},
								attrs: {
									...attrs,
									...context.data.attrs,
								},
								props: {
									...getSize(props),
									nuxt: true,
									raised: true,
									color: getColor(props, isDark),
									textId: props.textId,
									block: props.block,
									disabled: props.disabled,
									outlined: props.outlined,
									loading: props.loading,
									to: props.to,
									target: props.href ? '_blank' : '_self',
									width: getWidth(props),
									height: getHeight(props),
								},
							},
							[getButtonContentsElement(h, context, isDark)],
						);
					},
				},
			},
			[toolTipSpanElement(h, context)],
		);
	},
});

function getButtonText(context: RenderContext): string {
	return context.props.textId ? t(context, `general.commands.${context.props.textId}`) : '';
}

function getButtonContentsElement(h: CreateElement, context: RenderContext, isDark: boolean): VNode | VNodeChildren {
	const buttonText = getButtonText(context);
	const elements: VNodeChildren = [];
	if (!context.props.iconOnly) {
		elements.push(h('span', {}, buttonText));
	}

	const iconElement = iconSpanElement(h, context.props as IBaseButtonProps, isDark);
	if (!iconElement) {
		return elements;
	}
	if (context.props.iconAlign === 'Left') {
		elements.unshift(iconElement);
	}
	if (context.props.iconAlign === 'Right') {
		elements.push(iconElement);
	}
	return elements;
}

function toolTipSpanElement(h: CreateElement, context: RenderContext) {
	if (!context.props.tooltipId) {
		return null;
	}
	return h('span', {}, t(context, context.props.tooltipId));
}

function iconSpanElement(h: CreateElement, props: IBaseButtonProps, isDark: boolean) {
	let icon = '';
	if (props.icon) {
		icon = props.icon;
	} else if (props.type) {
		icon = Convert.buttonTypeToIcon(props.type as ButtonType);
	} else {
		return null;
	}
	return h(
		VIcon,
		{
			class: 'mx-2',
			props: {
				size: getIconSize(props),
				color: getColor(props, isDark),
			},
		},
		[h('template', { slot: 'default' }, icon)],
	);
}

function getWidth(props: IBaseButtonProps): Number | string | undefined {
	if (props.iconOnly) {
		return undefined;
	}
	return props.width !== 36 ? props.width : 'auto';
}

function getHeight(props: IBaseButtonProps) {
	if (props.size !== 'normal') {
		return undefined;
	}
	return props.height;
}

function getIconSize(props: IBaseButtonProps): number | undefined {
	return props.iconOnly ? 32 : undefined;
}

function getSize(props: IBaseButtonProps) {
	return {
		xSmall: props.size === 'x-small',
		small: props.size === 'small',
		large: props.size === 'large',
		xLarge: props.size === 'x-large',
	};
}

function getColor(props: IBaseButtonProps, isDark: boolean): string {
	if (props.color) {
		return props.color;
	}
	return isDark ? props.darkColor : props.lightColor;
}

function t(context: RenderContext, tag: string): string {
	return context.parent.$t(tag).toString();
}
</script>
