<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import IconButton from './IconButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'HelpButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			IconButton,
			{
				...mergeProps(this.$props, {
					icon: 'mdi-help-circle-outline',
					size: 'md',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
