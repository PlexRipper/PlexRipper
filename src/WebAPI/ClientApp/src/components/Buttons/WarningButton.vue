<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'WarningButton',
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
				color: 'yellow darken-1',
				outlined: true,
				icon: 'mdi-alert',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
