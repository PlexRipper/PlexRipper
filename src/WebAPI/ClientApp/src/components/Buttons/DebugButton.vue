<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'DebugButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					icon: 'mdi-bug-outline',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
