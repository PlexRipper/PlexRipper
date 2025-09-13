<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import type { IBaseButtonProps } from '@props';
import { useI18n } from 'vue-i18n';
import BaseButton from './BaseButton.vue';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'ConfirmButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {
					color: 'positive',
					label: this.$props.label ? this.$props.label : useI18n().t(`general.commands.confirm`),
					icon: 'mdi-check',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
