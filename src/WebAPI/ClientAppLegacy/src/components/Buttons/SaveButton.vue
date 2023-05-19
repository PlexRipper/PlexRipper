<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'SaveButton',
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
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				outlined: true,
				icon: 'mdi-content-save',
				color: 'green',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
