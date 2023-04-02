<script lang="ts">
import { h, defineComponent, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationSkipSetupButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(BaseButton, {
			...mergeProps(this.$props, {
				textId: 'skip-setup',
				icon: 'mdi-debug-step-over',
				iconAlign: 'right',
			} as IBaseButtonProps),
		});
	},
});
</script>
