<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'CheckConnectionButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					tooltipId: 'general.commands.check-connection',
					icon: 'mdi-cloud-search-outline',
					iconOnly: true,
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
