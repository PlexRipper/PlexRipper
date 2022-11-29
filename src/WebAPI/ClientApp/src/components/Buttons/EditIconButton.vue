<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'EditIconButton',
	functional: true,
	props: {
		disabled: {
			type: Boolean,
		},
		cy: {
			type: String,
			default: '',
		},
		height: {
			type: Number,
			default: 50,
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				outlined: true,
				textId: 'edit',
				icon: 'mdi-pencil',
				iconOnly: true,
				iconSize: 32,
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
