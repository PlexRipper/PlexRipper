<script lang="ts">
import Vue, { VNode, CreateElement, PropType } from 'vue';
import { RenderContext } from 'vue/types/options';
import VBtn from 'vuetify/lib/components/VBtn';
import VTooltip from 'vuetify/lib/components/VTooltip';
import VIcon from 'vuetify/lib/components/VIcon';
import Log from 'consola';
import ButtonType from '@enums/buttonType';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import { GlobalService } from '@service';

export interface IBaseButtonProps {
	block: boolean;
	disable: boolean;
	outlined: boolean;
	textId: string;
	type: ButtonType | String;
	tooltipId: string;
	icon: string;
	iconSize: string;
	lightColor: string;
	darkColor: string;
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
		const icon = Convert.buttonTypeToIcon(context.props.type as ButtonType);
		const isDark = context.parent.$vuetify.theme.dark;
		const buttonText = getButtonText(context.props.textId);
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
								},
								on: {
									...on,
								},
								attrs: {
									...attrs,
									...context.data.attrs,
								},
								props: {
									nuxt: true,
									raised: true,
									textId: context.props.textId,
									block: context.props.block,
									disable: context.props.disable,
									outlined: context.props.outlined,
								},
							},
							[
								icon
									? h(
											VIcon,
											{
												class: 'mx-2',
												props: {
													size: context.props.iconSize,
													color: isDark ? context.props.darkColor : context.props.lightColor,
												},
											},
											[h('template', { slot: 'default' }, icon)],
									  )
									: null,
								h('span', {}, buttonText),
							],
						);
					},
				},
			},
			[h('span', {}, 'toolTipText')],
		);
	},
});

function getButtonText(textId: string): string {
	return textId ? GlobalService.translate(`general.commands.${textId}`) : 'MISSING TEXT';
}
</script>
