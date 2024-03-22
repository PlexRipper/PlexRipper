<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import type { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'AddIconButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					icon: 'mdi-plus-box-outline',
					size: 'xl',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
