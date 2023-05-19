<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import IconButton from './IconButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'ExternalLinkButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			IconButton,
			{
				...mergeProps(this.$props, {
					size: 'md',
					icon: 'mdi-open-in-new',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
