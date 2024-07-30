<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import IconButton from './IconButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';
import { useI18n } from '#imports';

export default defineComponent({
	name: 'CheckConnectionButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			IconButton,
			{
				...mergeProps(this.$props, {
					tooltipText: useI18n().t('general.commands.check-connection'),
					icon: 'mdi-cloud-search-outline',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
