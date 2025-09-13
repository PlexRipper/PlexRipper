<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import { useI18n } from 'vue-i18n';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'CancelButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					icon: 'mdi-alert',
					label: this.$props.label ? this.$props.label : useI18n().t(`general.commands.cancel`),
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
