<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'NavigationPreviousButton',
	functional: true,
	props: {
		disabled: {
			type: Boolean,
		},
		cy: {
			type: String,
			default: '',
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				block: true,
				outlined: true,
				textId: 'back',
				icon: 'mdi-arrow-left',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
