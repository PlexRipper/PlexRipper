<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'ExternalLinkButton',
	functional: true,
	props: {
		cy: {
			type: String,
			default: '',
		},
		width: {
			type: Number,
			default: 130,
		},
		textId: {
			type: String,
			default: '',
		},
		href: {
			type: String,
			default: '',
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				outlined: false,
				iconOnly: true,
				icon: 'mdi-open-in-new',
				iconSize: 24,
				size: 'small',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
