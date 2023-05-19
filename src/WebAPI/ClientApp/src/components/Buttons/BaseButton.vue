<script lang="ts">
import { defineComponent, h } from 'vue';
import { QBtn, QTooltip } from 'quasar';
import { useI18n, useRouter } from '#imports';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';
import { IBaseButtonProps } from '@props';

export default defineComponent({
	name: 'BaseButton',
	inheritAttrs: false,
	props: baseBtnPropsDefault(),
	emits: ['click'],
	render() {
		const props = this.$props as IBaseButtonProps;
		const emit = this.$emit;
		const slots = this.$slots;
		const style = {
			flat: props.flat,
			round: props.round,
			rounded: props.rounded,
			outline: props.outline,
		};

		let buttonText = props.label;
		if (props.textId) {
			buttonText = useI18n().t(`general.commands.${props.textId}`);
		}

		const classes = Object.assign(
			{
				'base-btn': true,
				'base-btn-outline': style.outline,
				'base-btn-block': props.block,
				'base-btn-vertical': props.vertical,
				[`base-btn-${props.color}`]: true,
				'i18n-formatting': true,
			},
			this.$attrs.class,
		);

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
				push: props.push,
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
			{
				default: () => [
					...(slots?.default?.() ?? []),
					...[
						props.tooltipId
							? h(
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
							  )
							: null,
					],
				],
			},
		);
	},
});
</script>
