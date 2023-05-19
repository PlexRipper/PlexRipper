<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'DebugButton',
	functional: true,
	props: {
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
			...context.data,
			props: {
				...context.props,
				outlined: true,
				icon: 'mdi-bug-outline',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
