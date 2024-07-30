<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';
import { useI18n } from '#imports';

export default defineComponent({
	name: 'NavigationPreviousButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					block: true,
					label: useI18n().t(`general.commands.back`),
					icon: 'mdi-arrow-left',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
