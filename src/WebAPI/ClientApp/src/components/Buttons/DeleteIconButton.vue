<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import IconButton from './IconButton.vue';
import type { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'DeleteIconButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			IconButton,
			{
				...mergeProps(this.$props, {
					size: 'lg',
					icon: 'mdi-delete',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
