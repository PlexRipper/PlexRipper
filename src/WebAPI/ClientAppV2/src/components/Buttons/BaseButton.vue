<script lang="ts">
import { h, resolveComponent, defineComponent } from 'vue';
import { useI18n, useRouter } from '#imports';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

const router = useRouter();
export default defineComponent({
	name: 'BaseButton',
	inheritAttrs: false,
	props: baseBtnPropsDefault(),
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
