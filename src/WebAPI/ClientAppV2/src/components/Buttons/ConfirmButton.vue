<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'ConfirmButton',
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
		loading: {
			type: Boolean,
			default: false,
		},
	},
	render(h: CreateElement, context: RenderContext): VNode {
		return h(BaseButton, {
			...context.data,
			props: {
				...context.props,
				outlined: true,
				color: 'green',
				textId: 'confirm',
				icon: 'mdi-check',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
