<script lang="ts">
import { h, resolveComponent, defineComponent } from 'vue';
import { useI18n, useRouter } from '#imports';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'BaseButton',
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
		if (props.iconOnly && props.flat) {
			style.flat = true;
		}

		let buttonText = props.label;
		if (props.textId) {
			buttonText = useI18n().t(`general.commands.${props.textId}`);
		}
		// Fallback text
		if (!props.iconOnly && !buttonText) {
			buttonText = useI18n().t(`general.commands.missing-text`);
		}

		const classes = {
			'base-btn': true,
			'base-btn-outline': style.outline,
			'base-btn-block': props.block,
			[`base-btn-${props.color}`]: true,
			'i18n-formatting': true,
			...this.$attrs.class,
		};
		return h(
			QBtn,
			{
				class: classes,
				label: buttonText,
				icon: props.iconAlign === 'left' ? props.icon : undefined,
				iconRight: props.iconAlign === 'right' ? props.icon : undefined,
				glossy: props.glossy,
				size: props.size,
				loading: props.loading,
				disable: props.disabled,
				'data-cy': props.cy,
				flat: style.flat,
				round: style.round,
				rounded: style.rounded,
				href: props.href ? props.href : undefined,
				target: props.href ? '_blank' : '_self',
				onClick(event: MouseEvent) {
					if (props.to) {
						useRouter().push({
							path: props.to,
						});
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
								default: () => {
									if (!props.tooltipId) {
										return '';
									}
									return useI18n().t(props.tooltipId);
								},
							},
						),
					];
				}
				return [];
			},
		);
	},
});
</script>
