<script lang="ts">
import { h, defineComponent, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'HelpButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(BaseButton, {
			...mergeProps(this.$props, {
				icon: 'mdi-help-circle-outline',
				iconOnly: true,
				size: 'md',
			}),
		} as Partial<IBaseButtonProps>);
	},
});
</script>
