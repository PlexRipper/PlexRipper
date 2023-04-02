<script lang="ts">
import { h, defineComponent, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'ExternalLinkButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(BaseButton, {
			...mergeProps(this.$props, {
				iconOnly: true,
				size: 'md',
				icon: 'mdi-open-in-new',
			}),
		} as Partial<IBaseButtonProps>);
	},
});
</script>
