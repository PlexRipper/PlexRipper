<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'HelpButton',
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
				outlined: false,
				icon: 'mdi-help-circle-outline',
				iconOnly: true,
				iconSize: 24,
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
