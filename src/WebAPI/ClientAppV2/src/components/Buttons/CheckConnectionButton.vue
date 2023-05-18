<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import IconButton from './IconButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'CheckConnectionButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			IconButton,
			{
				...mergeProps(this.$props, {
					tooltipId: 'general.commands.check-connection',
					icon: 'mdi-cloud-search-outline',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
