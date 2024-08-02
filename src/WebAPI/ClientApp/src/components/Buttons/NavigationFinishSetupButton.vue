<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';
import { useI18n } from '#imports';

export default defineComponent({
	name: 'NavigationFinishSetupButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					label: useI18n().t(`general.commands.finish-setup`),
					icon: 'mdi-check-circle-outline',
					iconAlign: 'right',
					color: 'positive',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
