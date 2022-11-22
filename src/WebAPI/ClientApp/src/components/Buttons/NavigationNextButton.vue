<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';
import ButtonType from '@enums/buttonType';

export default Vue.extend({
	name: 'NavigationNextButton',
	functional: true,
	props: {
		block: {
			type: Boolean,
		},
		disabled: {
			type: Boolean,
		},
		cy: {
			type: String,
			default: '',
		},
		textId: {
			type: String,
			default: '',
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			props: {
				disabled: context.props.disabled,
				block: true,
				outlined: true,
				textId: 'next',
				type: ButtonType.Forward,
				tooltipId: 'asdads',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
