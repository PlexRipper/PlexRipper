<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'NavigationSkipSetupButton',
	functional: true,
	props: {
		disabled: {
			type: Boolean,
		},
		cy: {
			type: String,
			default: '',
		},
		width: {
			type: Number,
			default: 36,
		},
		block: {
			type: Boolean,
			default: true,
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				outlined: true,
				textId: 'skip-setup',
				icon: 'mdi-debug-step-over',
				iconAlign: 'Right',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
