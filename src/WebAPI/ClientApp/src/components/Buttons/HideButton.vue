<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import { useI18n } from 'vue-i18n';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'HideButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					label: useI18n().t(`general.commands.hide`),
					icon: 'mdi-eye-off-outline',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
