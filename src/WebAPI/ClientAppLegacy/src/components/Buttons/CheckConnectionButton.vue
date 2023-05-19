<script lang="ts">
import Vue, { CreateElement, VNode } from 'vue';
import { RenderContext } from 'vue/types/options';
import BaseButton, { IBaseButtonProps } from './BaseButton.vue';

export default Vue.extend({
	name: 'CheckConnectionButton',
	functional: true,
	props: {
		disabled: {
			type: Boolean,
		},
		cy: {
			type: String,
			default: '',
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
				iconAlign: 'Right',
				iconOnly: true,
				outlined: false,
				icon: 'mdi-cloud-search-outline',
				tooltipId: 'general.commands.check-connection',
			} as Partial<IBaseButtonProps>,
		});
	},
});
</script>
