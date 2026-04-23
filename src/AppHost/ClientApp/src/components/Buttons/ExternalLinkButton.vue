<script lang="ts">
import { defineComponent, h } from 'vue';
import type { IBaseButtonProps } from '@props';
import ExternalLink from '@components/Common/ExternalLink.vue';
import IconButton from './IconButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'ExternalLinkButton',
	props: baseBtnPropsDefault(),
	render() {
		const props = this.$props as IBaseButtonProps;

		return h(
			ExternalLink,
			{
				href: props.href,
				cy: props.cy,
			},
			{
				default: () => h(IconButton, {
					...props,
					href: undefined,
					cy: undefined,
					size: 'md',
					icon: 'mdi-open-in-new',
				}),
			},
		);
	},
});
</script>
