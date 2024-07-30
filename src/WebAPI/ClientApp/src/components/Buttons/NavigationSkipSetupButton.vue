<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationSkipSetupButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					textId: 'skip-setup',
					icon: 'mdi-debug-step-over',
					iconAlign: 'right',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
