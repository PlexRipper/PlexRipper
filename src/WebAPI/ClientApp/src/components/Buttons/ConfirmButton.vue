<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
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
					textId: 'confirm',
					icon: 'mdi-check',
				} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
