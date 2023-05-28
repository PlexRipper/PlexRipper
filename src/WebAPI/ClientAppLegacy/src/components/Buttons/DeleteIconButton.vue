<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'DeleteIconButton',
	functional: true,
	props: {
		disabled: {
			type: Boolean,
		},
		cy: {
			type: String,
			default: '',
		},
		iconOnly: {
			type: Boolean,
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				outlined: true,
				textId: 'delete',
				icon: 'mdi-delete',
				color: 'red',
				iconOnly: true,
				iconSize: 32,
				height: 50,
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
