<script lang="ts">
import { defineComponent, h, mergeProps } from 'vue';
import BaseButton from './BaseButton.vue';
import { IBaseButtonProps } from '@props';
import { baseBtnPropsDefault } from '~/composables/baseBtnProps';

export default defineComponent({
	name: 'AccountValidationButton',
	props: baseBtnPropsDefault(),
	render() {
		return h(
			BaseButton,
			{
				...mergeProps(this.$props, {} as IBaseButtonProps),
			},
			this.$slots.default,
		);
	},
});
</script>
