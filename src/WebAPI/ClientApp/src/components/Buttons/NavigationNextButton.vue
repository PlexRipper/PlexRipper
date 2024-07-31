<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';
import { useI18n } from '#imports';

export default defineComponent({
	name: 'NavigationNextButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...this.$attrs,
				...mergeProps(this.$props, {
					block: true,
					label: useI18n().t(`general.commands.next`),
					iconAlign: 'right',
					icon: 'mdi-arrow-right',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
