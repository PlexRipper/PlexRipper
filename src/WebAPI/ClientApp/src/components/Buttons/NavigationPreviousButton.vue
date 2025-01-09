<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import { useI18n } from 'vue-i18n';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'NavigationPreviousButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					block: true,
					label: this.$props.label ? this.$props.label : useI18n().t(`general.commands.back`),
					icon: 'mdi-arrow-left',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
