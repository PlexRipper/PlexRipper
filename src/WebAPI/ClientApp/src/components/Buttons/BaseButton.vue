<script lang="ts">
import Vue, { VNode, CreateElement, PropType } from 'vue';
import { RenderContext } from 'vue/types/options';
import VBtn from 'vuetify/lib/components/VBtn';
import VTooltip from 'vuetify/lib/components/VTooltip';
import VIcon from 'vuetify/lib/components/VIcon';
import Log from 'consola';
import ButtonType from '@enums/buttonType';
import Convert from '@mediaOverview/MediaTable/types/Convert';

export interface IBaseButtonProps {
	block: boolean;
	disable: boolean;
	outlined: boolean;
	filled: boolean;
	textId: string;
	type: ButtonType | String;
	tooltipId: string;
	icon: string;
	iconSize: string;
	lightColor: string;
	darkColor: string;
	href: string;
	to: string;
}

export default Vue.extend<IBaseButtonProps>({
	name: 'BaseButton',
	functional: true,
	props: {
		block: {
			type: Boolean,
		},
		disable: {
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
	},
	render(h: CreateElement, context: RenderContext<IBaseButtonProps>): VNode {
		const isDark = context.parent.$vuetify.theme.dark;
		const buttonText = getButtonText(context);

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
									filled: context.props.filled,
									outlined: context.props.outlined,
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
									color: isDark ? context.props.darkColor : context.props.lightColor,
									textId: context.props.textId,
									block: context.props.block,
									disable: context.props.disable,
									outlined: context.props.outlined,
									to: context.props.to,
									target: context.props.href ? '_blank' : '_self',
								},
							},
							[iconSpanElement(h, context, isDark), h('span', {}, buttonText)],
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

function toolTipSpanElement(h: CreateElement, context: RenderContext) {
	if (!context.props.tooltipId) {
		return null;
	}
	return h('span', {}, t(context, context.props.tooltipId));
}

function iconSpanElement(h: CreateElement, context: RenderContext, isDark: boolean) {
	const icon = Convert.buttonTypeToIcon(context.props.type as ButtonType);
	if (!icon) {
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
