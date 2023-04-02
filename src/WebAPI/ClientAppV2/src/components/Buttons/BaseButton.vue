<script lang="ts">
import { h, resolveComponent, defineComponent } from 'vue';
import type { PropType } from 'vue';
import { useI18n, useRouter } from '#imports';
import { IBaseButtonProps } from '@props';
import ButtonType from '@enums/buttonType';

const router = useRouter();
export default defineComponent({
	name: 'BaseButton',
	inheritAttrs: false,
	props: {
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
			default: 'missing-text',
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
			default: true,
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
		},
	},

	emits: ['click'],
	render() {
		const QBtn = resolveComponent('QBtn');
		const QTooltip = resolveComponent('QTooltip');
		const props = this.$props;
		const emit = this.$emit;
		const style = {
			flat: props.flat,
			round: props.round,
			rounded: props.rounded,
			outline: props.outline,
		};
		if (props.iconOnly) {
			style.flat = false;
			style.round = true;
			style.rounded = true;
			style.outline = false;
		}
		const classes = {
			'base-btn': true,
			'base-btn-outline': style.outline,
			[`base-btn-${props.color}`]: true,
			'i18n-formatting': true,
		};
		return h(
			QBtn,
			{
				class: classes,
				label: getButtonText(props),
				icon: props.iconAlign === 'left' ? props.icon : undefined,
				iconRight: props.iconAlign === 'right' ? props.icon : undefined,
				glossy: props.glossy,
				size: props.size,
				loading: props.loading,
				disabled: props.disabled,
				dataCy: props.cy,
				flat: style.flat,
				round: style.round,
				rounded: style.rounded,
				onClick(event: MouseEvent) {
					if (props.to) {
						router.push(props.to);
					} else {
						emit('click', event);
					}
				},
			},
			() => {
				if (props.tooltipId) {
					return [
						h(
							QTooltip,
							{
								anchor: 'top middle',
								self: 'bottom middle',
								offset: [10, 10],
							},
							{
								default: () => getTooltipText(props),
							},
						),
					];
				}
				return [];
			},
		);
	},
});

function getButtonText(props: IBaseButtonProps): string | number {
	return props.textId ? useI18n().t(`general.commands.${props.textId}`) : props.label;
}

function getTooltipText(props: IBaseButtonProps): string {
	if (!props.tooltipId) {
		return '';
	}
	return useI18n().t(props.tooltipId);
}
</script>
