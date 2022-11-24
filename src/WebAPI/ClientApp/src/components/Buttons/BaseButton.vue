<script lang="ts">
import Vue, { VNode, CreateElement, PropType, VNodeChildren } from 'vue';
import { RenderContext } from 'vue/types/options';
import VBtn from 'vuetify/lib/components/VBtn';
import VTooltip from 'vuetify/lib/components/VTooltip';
import VIcon from 'vuetify/lib/components/VIcon';
import ButtonType from '@enums/buttonType';
import Convert from '@mediaOverview/MediaTable/types/Convert';

export interface IBaseButtonProps {
	block: boolean;
	disabled: boolean;
	outlined: boolean;
	filled: boolean;
	textId: string;
	type: ButtonType | String;
	tooltipId: string;
	icon: string;
	iconSize: string;
	iconAlign: 'Left' | 'Right';
	lightColor: string;
	darkColor: string;
	href: string;
	to: string;
	width: number;
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
		},
		filled: {
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
			type: String,
			default: '',
		},
		iconAlign: {
			type: String as PropType<'Left' | 'Right'>,
			default: 'Left',
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
	},
	render(h: CreateElement, context: RenderContext<IBaseButtonProps>): VNode {
		const isDark = context.parent.$vuetify.theme.dark;
		const props = context.props;
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
									nuxt: true,
									raised: true,
									color: isDark ? props.darkColor : props.lightColor,
									textId: props.textId,
									block: props.block,
									disabled: props.disabled,
									outlined: props.outlined,
									to: props.to,
									target: props.href ? '_blank' : '_self',
									width: props.width !== 36 ? props.width : 'auto',
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
	return context.props.textId ? t(context, `general.commands.${context.props.textId}`) : 'MISSING TEXT';
}

function getButtonContentsElement(h: CreateElement, context: RenderContext, isDark: boolean): VNode | VNodeChildren {
	const buttonText = getButtonText(context);
	const elements: VNodeChildren = [h('span', {}, buttonText)];

	const iconElement = iconSpanElement(h, context, isDark);
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

function iconSpanElement(h: CreateElement, context: RenderContext, isDark: boolean) {
	let icon = '';
	if (context.props.icon) {
		icon = context.props.icon;
	} else if (context.props.type) {
		icon = Convert.buttonTypeToIcon(context.props.type as ButtonType);
	} else {
		return null;
	}
	return h(
		VIcon,
		{
			class: 'mx-2',
			props: {
				size: context.props.iconSize,
				color: isDark ? context.props.darkColor : context.props.lightColor,
			},
		},
		[h('template', { slot: 'default' }, icon)],
	);
}

function t(context: RenderContext, tag: string): string {
	return context.parent.$t(tag).toString();
}
</script>
